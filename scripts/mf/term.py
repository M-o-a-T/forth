#!/usr/bin/env python
#
# Very simple serial terminal
#
# (c) 2021 Matthias Urlichs <matthias@urlichs.de>
#
# Part of this file is copied from pySerial.
# https://github.com/pyserial/pyserial (C)2002-2020 Chris Liechti
# <cliechti@gmx.net>
#
# SPDX-License-Identifier:    BSD-3-Clause

from __future__ import absolute_import

import codecs
import os
import io
import re
import stat
import sys
import anyio
import math
import subprocess
from contextlib import asynccontextmanager, contextmanager
from anyio.streams.stapled import StapledByteStream
from anyio_serial import Serial
from click.exceptions import UsageError
from .dummy import SendFile, StopSendFile, Data, SendBuffer
from pathlib import Path

from serial.tools import hexlify_codec

# pylint: disable=wrong-import-order,wrong-import-position

codecs.register(lambda c: hexlify_codec.getregentry() if c == 'hexlify' else None)

from serial.tools.miniterm import (
    EOL_TRANSFORMATIONS,
    TRANSFORMATIONS,
)

class EarlyEOFError(EOFError):
    pass
class AllEOFError(EOFError):
    pass
class ForthError(RuntimeError):
    pass
class ScriptError(RuntimeError):
    pass
Errors = (ForthError,ScriptError,UsageError,TimeoutError,EnvironmentError,ValueError,KeyError)

class AsyncDummy:
    def __init__(self,val):
        self.val = val
    async def __aenter__(self):
        return self.val
    async def __aexit__(self, *tb):
        self.val = None

class _MsgOut:
    msg = "??"
    line = None
    timeout = False
    good = None

    def __init__(self, s):
        self.line = s
    def __repr__(self):
        return f"<{self.__class__.__name__} line={self.line!r} msg={self.msg!r} t={self.timeout} ok={self.good}>"

def green(s):
    return f"\x1b[32m{s}\x1b[39m"
def bold(s):
    return f"\x1b[1m{s}\x1b[0m"

class WithNothing(_MsgOut):
    msg = ""
    timeout = True
class WithTimeout(_MsgOut):
    msg = "🕠"
    good = False
class WithOK(_MsgOut):
    msg = green("✓")
    good = True
class WithACK(_MsgOut):
    msg = green("🗸")
    good = True
class WithNAK(_MsgOut):
    msg = "❌"
    good = False
class WithEnd(_MsgOut):
    msg = "🔚"

class SendLine:
    def __init__(self, line):
        self.line = line
    def __repr__(self):
        return f"<{self.__class__.__name__} line={self.line!r}>"

class MsgLine(SendLine):
    def __init__(self, line, timeout):
        super().__init__(line)
        self.timeout = timeout
        self.recv_w,self.recv_r = anyio.create_memory_object_stream(10)

    def __repr__(self):
        return f"<{self.__class__.__name__} line={self.line!r} tm={self.timeout!r}>"

    async def send(self,evt):
        await self.recv_w.send(evt)

    def __aiter__(self):
        return self.recv_r.__aiter__()

def utflen(b):
    bit = 8-(b^255).bit_length()
    if bit < 2: # 0xxx_xxxx == 1, # 10xx_xxxx, continuation == 0
        return 1-bit
    else: # number of initial 1-bits == sequence length
        return bit

class Terminal:
    """\
    Terminal application. Copy data from command stdin/stdout to console and vice versa.
    Handle special keys from the console to show menu etc.
    """

    alive = None
    stream = None
    reader_task = None
    rx_decoder = None
    tx_decoder = None
    goahead_buf:str = ""
    goahead_delay:float = 0.5
    layer:int = 0 # skipped nested '#if…' statements
    layer_:int = 0 # total nested '#if…' statements
    console = None
    log = None
    _data = b""
    _exc = None
    port = None
    command = None
    file_sender = None
    file_ended = False

    _subst = re.compile('{[^}]+}')

    def subst_flags(self, line):
        def sub(m):
            return self.flags[m.group(0)[1:-1]]
        return self._subst.sub(sub, line)

    def __init__(self, command=False, port=("/dev/ttyUSB0","115200"), name=None, echo=False, eol='lf', filter=(), go_ahead = None, go_ack=None, go_nak=None, batch=None, log=None, develop=False, verbose=1, flag=(), ack=-1,nak=-1, timeout=0.2, reset=None, inv_reset=None, exec=(), bold=False):
        if command:
            self.command = port
        else:
            self.port = port[0]
            self.speed = int(port[1]) if len(port) > 1 else 115200
        self.name = name or '?'
        self.echo = echo
        self.eol = eol
        self.bold = bold
        self.filters = filter
        self.update_transformations()
        self.exit_character = chr(0x1D)  # GS/CTRL+]
        self.menu_character = chr(0x14)  # Menu: CTRL+T
        if isinstance(go_ahead,str):
            go_ahead = go_ahead.encode("utf-8")
        self.go_ahead = go_ahead
        self.go_ack = None if ack == -1 else ack
        self.go_nak = None if nak == -1 else nak
        if go_ack is not None and self.go_ahead is not None:
            self.go_check = None
        else:
            self.go_check = self.go_ahead is not None
        self.go_lf = 10
        self.go_cr = 13
        self.raw_in_w,self.raw_in_r = anyio.create_memory_object_stream(10)
        self.logfile = log
        self.develop = develop
        self.verbose = verbose
        self.goahead_delay = timeout
        self.rx_decoder = codecs.getincrementaldecoder("utf-8")("replace")
        self.tx_encoder = codecs.getincrementalencoder("utf-8")("replace")
        self.reset = reset,inv_reset
        self.batch = batch
        self.files = exec

        if batch is None:
            batch = not sys.stdin.isatty()
        if batch:
            if not exec:
                raise UsageError("You need a file to send if you send a batch job")
            from .dummy import NoWindow
            self.console = NoWindow()
        else:
            from .gtk import Window
            self.console = Window(self.port if self.port else self.command[0] if self.command else "Stream")

        self.flags = {}
        for fl in flag:
            try:
                fl,v = fl.split("=",1)
            except ValueError:
                v="-1"
            self.flags[fl] = v

    def _stop_reader(self):
        """Stop reader thread only, wait for clean exit of thread"""
        self.raw_reader_task.cancel()
        self.reader_task.cancel()


    async def _out_wrapper(self):
        if self.port:
            return Serial(self.port, self.speed)
        elif self.command:
            return await anyio.open_process(self.command, stdin=subprocess.PIPE, stdout=subprocess.PIPE)
        else:
            return AsyncDummy(self.stream)

    async def run(self):
        proc = None
        async with anyio.create_task_group() as tg:
            self.tg = tg
            if self.logfile is not None:
                self.log = await anyio.open_file(self.logfile,"w")
            async with await self._out_wrapper() as proc:
                if self.port:
                    self.stream = proc
                    self.stream.dtr = True
                    reset,inv_reset = self.reset
                    if reset:
                        self.stream.rts = not inv_reset
                        await anyio.sleep(0.1)
                    self.stream.rts = bool(inv_reset)

                elif self.command is not None:
                    self.stream = StapledByteStream(proc.stdin, proc.stdout)
                elif self.port is not None:
                    self.stream = proc
                else:
                    assert self.stream == proc

                await self.start()

                try:
                    if self.files:
                        await self.tg.start(self.upload_file, *self.files)
                    await anyio.sleep(0.1)
                    while True:
                        await anyio.sleep(99999)

                except Errors as e:
                    self.console.set_error(repr(e))
                    if not self.develop:
                        raise
                    self.console.set_error(e)
                except Exception as exc:
                    self._exc = exc
                finally:
                    with anyio.move_on_after(2, shield=True):
                        await self.stop()
                        tg.cancel_scope.cancel()
        if proc is not None and hasattr(proc,"returncode"):
            if proc.returncode is None:
                proc.kill()
            elif proc.returncode < 0:
                raise ForthError(f"SIGNAL {-proc.returncode}")
            elif proc.returncode > 0:
                raise ForthError(f"EXIT {proc.returncode}")
        if self._exc is not None:
            exc,self._exc = self._exc,None
            raise exc

    async def start(self):
        """start worker threads"""
        self.alive = True

        if self.log is not None:
            await self.tg.start(self.logger)
        await self.tg.start(self.reader)
        await self.tg.start(self.raw_reader)
        await self.tg.start(self.writer)

    async def writer(self, *, task_status):
        task_status.started()
        async for msg in self.console:
            if isinstance(msg,str):
                msg = SendLine(msg)
            elif isinstance(msg, StopSendFile):
                if self.file_sender is not None:
                    self.file_sender.cancel()
                continue
            elif isinstance(msg, SendFile):
                await self.tg.start(self.upload_file, msg.data)
                continue
            elif isinstance(msg, SendBuffer):
                await self.tg.start(self.upload_buffer, msg.data)
                continue
            elif isinstance(msg,Data):
                msg = SendLine(msg.data)
            else:
                print("Unknown Msg",repr(msg))
                continue

            await self.raw_in_w.send(msg)


    async def logger(self, *, task_status):
        self.log_w,log_r = anyio.create_memory_object_stream(10)
        task_status.started()
        with anyio.CancelScope(shield=True):
            async for line in log_r:
                await self.log.write(line)
            await self.log.aclose()

    async def stop(self):
        """set flag to stop worker threads"""
        self.alive = False
        if self.stream is not None:
            await self.stream.aclose()
        if self.console is not None:
            self.console.close()
        if self.log is not None:
            await self.log_w.aclose()

    def join(self, transmit_only=False):
        """wait for worker threads to terminate"""
        self.transmitter_thread.join()
        if not transmit_only:
            self.receiver_thread.join()

    def update_transformations(self):
        """take list of transformation classes and instantiate them for rx and tx"""
        transformations = [EOL_TRANSFORMATIONS[self.eol]] + [
            TRANSFORMATIONS[f] for f in self.filters
        ]
        self.tx_transformations = [t() for t in transformations]
        self.rx_transformations = list(reversed(self.tx_transformations))

    async def chat(self, text, timeout=False):
        """Send a line, return whatever was printed"""
        msg = MsgLine(text, timeout)
        await self.raw_in_w.send(msg)
        async for m in msg:
            if m.good:
                break
            if m.good is None:
                continue

            # error case
            if isinstance(m,WithTimeout):
                raise TimeoutError(m.msg)
            else:
                raise ForthError(m.line.strip())

        text = text.rstrip()
        line = m.line
        # filter echo. XXX do this in the reader instead?
        if self.go_ahead and line.startswith(text):
            line = line[len(text):]

        return line

    async def raw_reader(self, *, task_status):
        """loop and copy serial->console"""
        task_status.started()
        try:
            while True:
                if self.stream is None:
                    return
                data = await self.stream.receive(4096)
                if data == b"":
                    raise anyio.EndOfStream()
                await self.raw_in_w.send(data)
        except (anyio.ClosedResourceError,anyio.EndOfStream):
            await self.raw_in_w.send(WithEnd)

    async def reader(self, *, task_status):
        """loop and copy serial->console"""
        task_status.started()

        msg = None

        async def out(cls):
            nonlocal line, prev_len, do_wait, msg, need_lf
            line = self.rx_decoder.decode(line)
            self.console.send((bold(line) if self.bold else line) + cls.msg, lf=need_lf)
            need_lf = True
            if line and self.log is not None:
                await self.log_w.send(line+"\n")
            if msg is not None:
                await msg.send(cls(line))
                if cls.good is not None:
                    msg = None

            line = bytearray()
            prev_len = 0
            do_wait = cls.timeout

        line = bytearray()
        prev_len = 0
        do_wait = False

        in_utf = 0
        go_late = False
        need_lf = False

        # Theory of operation.
        # We can either wait for "ok." / timeout (go_check is True), or
        # for ack/nak (go_check is False), or we might not know which
        # (go_check is None). In the latter case we recognize the
        # "ok." after a timeout and set `self.go_check` so that we
        # won't have to wait again; conversely when we see an ack/nack
        # we clear `self.go_check` so we won't delay unnecessarily.
        # 
        while True:
            try:
                with anyio.fail_after(self.goahead_delay if do_wait or go_late else math.inf):
                    data = await self.raw_in_r.receive()
            except TimeoutError:
                need_lf = False
                if go_late:
                    if line.endswith(self.go_ahead):
                        self.go_check = True
                        await out(WithOK)
                    else:
                        await out(WithNothing)
                    go_late = False
                    continue
                await out(WithNAK if self.go_ahead is False else WithTimeout)
                continue
            except (anyio.EndOfStream, anyio.ClosedResourceError):
                data = WithEnd
            else:
                if isinstance(data,MsgLine):
                    if msg is not None:
                        await msg.send(WithEnd("collision"))
                    msg = data
                    # fall thru
                if isinstance(data,SendLine):
                    if self.log is not None:
                        await self.log_w.send("➤"+data.line+"\n")

                    if not self.go_check:
                        need_lf = False
                        self.console.send(data.line + "\u2003", lf=True) # em space
                    await self.send_line(data.line)
                    continue

            if data is WithEnd:
                await out(WithEnd)
                return

            for b in data:
                if b == self.go_lf:
                    if self.go_check is None:
                        go_late = True
                    elif self.go_check and line.endswith(self.go_ahead):
                        line = line[:-len(self.go_ahead)].rstrip()
                        await out(WithOK)
                        need_lf = True
                    else:
                        await out(WithNothing)
                else:
                    do_wait = True
                    if b == self.go_cr:
                        pass
                    elif self.go_ack is not None and b == self.go_ack and (b < 32 or not in_utf):
                        self.go_check = False
                        await out(WithACK)
                    elif self.go_nak is not None and b == self.go_nak and (b < 32 or not in_utf):
                        self.go_check = False
                        await out(WithNAK)
                    elif b >= 32:
                        if go_late:
                            await out(WithNothing)
                            go_late = False
                        if need_lf:
                            self.console.send("\n")
                            need_lf = False
                        line.append(b)
                        if in_utf and not utflen(b): # continuation
                            in_utf -= 1
                        else: # the codec will complain if utflen(b) is zero
                            in_utf = max(utflen(b)-1,0)
                    else:
                        # some random control character
                        line.extend(chr(0x2400+b).encode("utf-8"))

    async def preprocess(self, line, fn,li):
        """Filter lines read from a file.
        """
        line = line.strip()
        if line == "" or line == "\\" or line.startswith("\\ "):
            return

        oline = line
        if line[0] == '#':

            try:
                code,line = line.split(None, 1)
            except ValueError:
                code = line
                line = ""
            code = code[1:]
        else:
            code = "-"

        if code == "if":
            self.layer_ += 1
            if self.layer:
                self.layer += 1
                return
            line = self.subst_flags(line)
            res = await self.chat(f"{line} .", timeout=True)
            if not int(res.strip()):
                self.layer = 1
            return
        if code == "[if]":
            self.layer_ += 1
            if self.layer:
                self.layer += 1
                return
            line = self.subst_flags(line)
            res = await self.chat(f"[ {line} . ]", timeout=True)
            if not int(res.strip()):
                self.layer = 1
            return
        if code == "if-ok":
            self.layer_ += 1
            if self.layer:
                self.layer += 1
                return
            line = self.subst_flags(line)
            try:
                res = await self.chat(f"{line} .", timeout=True)
            except TimeoutError:
                self.layer = 1
            return
        if code == "if-ram":
            self.layer_ += 1
            if self.layer:
                self.layer += 1
                return
            res = await self.chat(f"compiletoram? compiletoram .", timeout=True)
            ram = int(res.strip())
            res = await self.chat(f"{line} .", timeout=True)
            if not int(res.strip()):
                self.layer = 1
            if not ram:
                await self.chat(f"compiletoflash", timeout=True)
            return
        if code == "if-flag":
            self.layer_ += 1
            if self.layer:
                self.layer += 1
                return
            for f in line.split():
                if f[0] == "!":
                    try:
                        f,v = f[1:].split("=",1)
                    except ValueError:
                        if f[1:] in self.flags:
                            break
                    else:
                        if self.flags.get(f,"") == v:
                            break
                else:
                    try:
                        f,v = f.split("=",1)
                    except ValueError:
                        if f not in self.flags:
                            break
                    else:
                        if self.flags.get(f,"") != v:
                            break
            else:  # no "break" was hit, all match
                return
            # some "break" was hit, mismatch
            self.layer = 1
            return

        if code == "else":
            if not self.layer_:
                raise ScriptError(f"'#else' without corresponding '#if…'")
            if self.layer <= 1:
                self.layer = 1-self.layer
            return
        if code == "endif" or code == "then":
            if self.layer:
                self.layer -= 1
            if self.layer_:
                self.layer_ -= 1
            else:
                self.layer = self.layer_ = 0
                raise ScriptError(f"'#{code}' without corresponding '#if…'")
            return

        if self.layer:
            return

        if code == "end":
            raise EarlyEOFError
        if code == "end*":
            raise AllEOFError
        if code == "echo":
            if self.verbose:
                line = self.subst_flags(line)
                self.console.send(f"{line}", lf=True)
            return
        if code == "ok":
            res = await self.chat(f"{line} .", timeout=True)
            if not int(res.strip()):
                raise ForthError("Check failed")
            return
        if code == "-ok":
            try:
                res = await self.chat(f"{line} .", timeout=True)
            except TimeoutError:
                return
            except ForthError:
                return
            else:
                raise ForthError("Did not fail")
        if code == "send":
            line = self.subst_flags(line)
            res = await self.chat(f"{line}", timeout=True)
            return
        if code == "error":
            raise ForthError(f"Error: {line}")
        if code == "read-flag":
            flag,expr = line.split(None, 1)
            expr = self.subst_flags(expr)
            self.flags[flag] = (await self.chat(f"{expr} .", timeout=True)).strip()
            return
        if code == "set-flag":
            try:
                flag,expr = line.split(None, 1)
            except ValueError:
                flag=line
                expr="1"
            if expr == "-":
                del self.flags[flag]
            else:
                self.flags[flag] = self.subst_flags(expr)
            return
        if code == "delay":
            self.goahead_delay = float(self.subst_flags(line))
            return
        if code == "include":
            await self.upsend_file(self.subst_flags(line))
            return
        if code == "require":
            try:
                word,fn = line.split(None, 1)
            except ValueError:
                word = line
                fn = f"lib/{word}.fs"
            else:
                if fn[-1] == '/':
                    fn += f"{word}.fs"
            res = await self.chat(f"undefined {word} .", timeout=True)
            if int(res.strip()):
                await self.upsend_file(self.subst_flags(fn))
            return

        # code not recognized: ordinary Forth word
        line = oline
        if line.endswith("  \\"):
            line = line[:-1]
        i = line.find(' \\ ')
        if i > -1:
            line = line[:i]
        line = line.strip()
        if not line:
            return

        return line

    async def send_line(self, line):
        """Send a single line."""
        await self.stream.send(self.tx_encoder.encode(line+"\n"))

    async def send_file(self, filename):
        """Send a file. Runs in upload_file task."""
        try:
            file_rel = str(Path(filename).resolve().relative_to(Path.cwd()))
        except ValueError:
            pass
        else:
            if len(filename) > len(file_rel):
                filename = file_rel

        async with await anyio.open_file(filename, 'r') as f:
            await self._file_send(filename, f.__aiter__())

    async def send_buffer(self, buf):
        async def sdr():
            for line in buf:
                i = line.find("\u2003")
                if i > -1:
                    line = line[:i]
                yield line
        await self._file_send("‹pastebuf›", sdr().__aiter__())

    async def _file_send(self, filename, lines):
        self.console.send(f'⮞ {filename} : start', lf=True)
        layer,self.layer_ = self.layer_,0
        num = 0
        self.file_ended = False
        try:
            while True:
                try:
                    num += 1
                    line = await lines.__anext__()
                    if line == "":
                        break
                    line = await self.preprocess(line,filename,num)
                    if self.file_ended:
                        self.console.send(f'⮞ {filename} : {num}', lf=True)
                        self.file_ended = False
                except AllEOFError as err:
                    self.console.send(f'⮞ {filename} : {num} (stop)', lf=True)
                    self.layer = self.layer_ = 0
                    raise
                except EarlyEOFError as err:
                    self.console.send(f'⮞ {filename} : {num} (exit)', lf=True)
                    self.layer, self.layer_ = 0,layer
                    return
                except (StopAsyncIteration,EOFError):
                    break
                if not line:
                    continue

                await self.chat(line, timeout=True)
                self.console.set_fileinfo(filename,num)

        except AllEOFError as err:
            # already printed above
            raise
        except Exception as exc:
            self.console.send(f'⮞ {filename} : {num} error', lf=True)
            raise

        self.file_ended = True
        if self.layer_:
            self.layer = self.layer_ = 0
            self.console.send(f'⮞ {filename} : end error', lf=True)
            raise ScriptError(f"{filename} : {num} '#if…' without corresponding '#endif'")
        self.layer_ = layer
        self.console.send(f'⮞ {filename} : end', lf=True)

    async def upsend_file(self, path):
        if self.file_sender is None:
            await self.tg.start(self.upload_file, path)
        else:
            await self.send_file(path)

    @contextmanager
    def upload_scope(self):
        if self.file_sender is not None:
            self.console.set_error(f'Already sending a file')
            return
        with anyio.CancelScope() as sc:
            self.file_sender = sc
            self.console.started_sending()
            try:
                yield self
            except AllEOFError as err:
                pass
            except Errors as e:
                if self.develop:
                    self.console.set_error(repr(e))
                else:
                    raise
            finally:
                self.file_sender = None
                self.layer_,self.layer = 0,0
                self.console.stopped_sending()
                if self.batch:
                    self.tg.cancel_scope.cancel()


    async def upload_buffer(self, buffer, task_status):
        with self.upload_scope():
            task_status.started()
            await self.send_buffer(buffer)

    async def upload_file(self, *files, task_status):
        """Ask user for filename and send its contents"""
        with self.upload_scope():
            task_status.started()

            for path in files:
                await anyio.sleep(0.1)
                await self.send_file(path)

    async def change_filter(self, filters=()):
        """change the i/o transformations"""
        if filters:
            for f in filters:
                if f not in TRANSFORMATIONS:
                    await self.term.show_line(f'\\ --- unknown filter: {f !r}')
                    break
            else:
                self.filters = filters
                self.update_transformations()
        await self.term.show_line(f'#filter {" ".join(self.filters)}')

    def change_goahead(self, goahead):
        """Change go-ahead sequence"""
        self.go_ahead = goahead
