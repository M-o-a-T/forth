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
import stat
import sys
import anyio
import subprocess
from contextlib import asynccontextmanager
from anyio.streams.stapled import StapledByteStream

from serial.tools import hexlify_codec

# pylint: disable=wrong-import-order,wrong-import-position

codecs.register(lambda c: hexlify_codec.getregentry() if c == 'hexlify' else None)

from serial.tools.miniterm import (
    EOL_TRANSFORMATIONS,
    TRANSFORMATIONS,
    Console,
    key_description,
)

class EarlyEOFError(EOFError):
    pass

class AsyncDummy:
    def __init__(self,val):
        self.val = val
    async def __aenter__(self):
        return self.val
    async def __aexit__(self, *tb):
        self.val = None

class Miniterm:
    """\
    Terminal application. Copy data from command stdin/stdout to console and vice versa.
    Handle special keys from the console to show menu etc.
    """

    alive = None
    stream = None
    reader_task = None
    rx_decoder = None
    tx_decoder = None
    line_buf:str = None
    goahead_buf:str = ""
    goahead_flag:anyio.Event = None
    goahead_delay:float = 0.3
    layer:int = 0 # skipped nested '#if…' statements
    layer_:int = 0 # total nested '#if…' statements
    console = None
    log = None
    _data = b""

    def __init__(self, command=None, stream=None, name=None, echo=False, eol='lf', filters=(), go_ahead = None, file=None, batch=None, logfile=None, develop=False):
        if bool(command) == bool(stream):
            raise RuntimeError("Specify one of 'command' and 'stream'")
        self.command = command
        self.stream = stream
        self.name = name or '?'
        self.echo = echo
        self.raw = False
        self.input_encoding = 'UTF-8'
        self.output_encoding = 'UTF-8'
        self.eol = eol
        self.filters = filters
        self.update_transformations()
        self.exit_character = chr(0x1D)  # GS/CTRL+]
        self.menu_character = chr(0x14)  # Menu: CTRL+T
        self.go_ahead = go_ahead
        self.file = file
        self.logfile = logfile
        self.develop = develop
        if batch is None:
            batch = not sys.stdin.isatty()
        if not batch:
            self.console = Console()
        elif not file:
            raise RuntimeError("You need a file to send if you send a batch job")

    def _stop_reader(self):
        """Stop reader thread only, wait for clean exit of thread"""
        self.reader_task.cancel()

    async def run(self):
        proc = None
        async with anyio.create_task_group() as tg:
            self.tg = tg
            self._closing = anyio.Event()
            if self.logfile is not None:
                self.log = await anyio.open_file(self.logfile,"w")
            async with AsyncDummy(self.stream) if self.stream is not None else await anyio.open_process(self.command, stdin=subprocess.PIPE, stdout=subprocess.PIPE) as proc:
                if self.stream is None:
                    self.stream = StapledByteStream(proc.stdin, proc.stdout)
                await self.start()
                try:
                    if self.file:
                        await anyio.sleep(0.1)
                        try:
                            await self.send_file(self.file)
                        except Exception as e:
                            if not self.develop:
                                raise
                            sys.stderr.write('\n--- ERROR: {} ---\n'.format(e))

                        self.file = None
                    if self.console is not None:
                        await self._closing.wait()
                    # otherwise we're done after the file is processed
                finally:
                    await self.stop()
                    tg.cancel_scope.cancel()
        if proc is not None and hasattr(proc,"returncode"):
            if proc.returncode is None:
                proc.kill()
            elif proc.returncode < 0:
                raise RuntimeError(f"SIGNAL {-proc.returncode}")
            elif proc.returncode > 0:
                raise RuntimeError(f"EXIT {proc.returncode}")

    async def start(self):
        """start worker threads"""
        self.alive = True

        if self.log is not None:
            await self.tg.start(self.logger)
        await self.tg.start(self.reader)
        # enter console->serial loop
        if self.console is not None:
            await self.tg.start(self.writer)
            self.console.setup()

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
        await self.stream.aclose()
        if self.console is not None:
            self.console.cancel()
        if self.log is not None:
            await self.log_w.aclose()

    def join(self, transmit_only=False):
        """wait for worker threads to terminate"""
        self.transmitter_thread.join()
        if not transmit_only:
            self.receiver_thread.join()

    async def aclose(self):
        if self.stream is not None:
            await self.stream.aclose()
            self.stream = None

    def update_transformations(self):
        """take list of transformation classes and instantiate them for rx and tx"""
        transformations = [EOL_TRANSFORMATIONS[self.eol]] + [
            TRANSFORMATIONS[f] for f in self.filters
        ]
        self.tx_transformations = [t() for t in transformations]
        self.rx_transformations = list(reversed(self.tx_transformations))

    def set_rx_encoding(self, encoding, errors='replace'):
        """set encoding for received data"""
        self.input_encoding = encoding
        self.rx_decoder = codecs.getincrementaldecoder(encoding)(errors)

    def set_tx_encoding(self, encoding, errors='replace'):
        """set encoding for transmitted data"""
        self.output_encoding = encoding
        self.tx_encoder = codecs.getincrementalencoder(encoding)(errors)

    def dump_port_settings(self):
        """Write current settings to sys.stderr"""
        p=self.stream

        try:
            sys.stderr.write(f"\n--- Port: {self.name}  {p.baudrate},{p.bytesize},{p.parity},{p.stopbits}\n")

            sys.stderr.write('--- RTS: {:8}  DTR: {:8}  BREAK: {:8}\n'.format(
                ('active' if self.stream.rts else 'inactive'),
                ('active' if self.stream.dtr else 'inactive'),
                ('active' if self.stream.break_condition else 'inactive')))
            sys.stderr.write('--- CTS: {:8}  DSR: {:8}  RI: {:8}  CD: {:8}\n'.format(
                ('active' if self.stream.cts else 'inactive'),
                ('active' if self.stream.dsr else 'inactive'),
                ('active' if self.stream.ri else 'inactive'),
                ('active' if self.stream.cd else 'inactive')))
        except AttributeError:
            pass
        sys.stderr.write(f"\n--- Settings: {self.name}\n")
        sys.stderr.write(f'--- serial input encoding: {self.input_encoding}\n')
        sys.stderr.write(f'--- serial output encoding: {self.output_encoding}\n')
        sys.stderr.write(f'--- EOL: {self.eol.upper()}\n')
        sys.stderr.write(f"--- filters: {' '.join(self.filters)}\n")

    async def chat(self, text, timeout=False):
        """Send a line, return whatever was printed"""
        if not self.go_ahead:
            raise RuntimeError("I need a go-ahead string")
        if not text.endswith("\n"):
            text += "\n"

        self.goahead_flag = anyio.Event()
        self.line_buf = ""

        await self.stream.send(self.tx_encoder.encode(text))
        with anyio.move_on_after(self.goahead_delay) if timeout is False else anyio.fail_after(self.goahead_delay) if timeout is True else anyio.fail_after(timeout):
            await self.goahead_flag.wait()

        line,self.line_buf = self.line_buf,None
        self.line_buf_old = line

        text = text.rstrip()
        if line.startswith(text):
            line = line[len(text):]
        self.goahead_flag = None

        return line

    async def reader(self, *, task_status):
        """loop and copy serial->console"""
        task_status.started()
        try:
            while True:
                # read all that is there or wait for one byte
                data = await self.stream.receive(4096)
                self._data += data
                if self.raw:
                    if self.console is not None:
                        self.console.write_bytes(data)
                    if self.log is not None:
                        await self.log_w.send(repr(data)+"\n")
                else:
                    text = self.rx_decoder.decode(data)
                    if not text:
                        continue
                    i = -1
                    if self.go_ahead and self.goahead_flag:
                        buf = self.goahead_buf + text
                        i = buf.find(self.go_ahead)
                        if i >= 0:
                            if self.line_buf is not None:
                                i += len(self.line_buf) - len(self.goahead_buf)
                            self.goahead_buf = ""
                            self.goahead_flag.set()
                        else:
                            self.goahead_buf = buf[-len(self.go_ahead):] if len(self.go_ahead)>1 else ""

                    for transformation in self.rx_transformations:
                        text = transformation.rx(text)
                    if self.console is not None:
                        self.console.write(text)
                    if self.line_buf is not None:
                        self.line_buf += text
                        if i >= 0:
                            self.line_buf = self.line_buf[:i]
                    if self.log is not None:
                        await self.log_w.send(text)

        except (anyio.EndOfStream, anyio.ClosedResourceError):
            return
        finally:
            self._closing.set()

    async def writer(self, *, task_status):
        task_status.started()
        await anyio.to_thread.run_sync(self._writer, cancellable=True)
        self._closing.set()

    def _writer(self):
        """\
        Loop and copy console->serial until self.exit_character character is
        found. When self.menu_character is found, interpret the next key
        locally.
        """
        menu_active = False
        while True:
            try:
                c = self.console.getkey()
            except KeyboardInterrupt:
                c = '\x03'
            if not self.alive:
                break
            if menu_active:
                self.handle_menu_key(c)
                menu_active = False
            elif c == self.menu_character:
                menu_active = True  # next char will be for menu
            elif c == self.exit_character:
                break
            else:
                # ~ if self.raw:
                text = c
                for transformation in self.tx_transformations:
                    text = transformation.tx(text)
                anyio.from_thread.run(self.stream.send, self.tx_encoder.encode(text))
                if self.echo:
                    echo_text = c
                    for transformation in self.tx_transformations:
                        echo_text = transformation.echo(echo_text)
                    self.console.write(echo_text)

    def handle_menu_key(self, c):
        """Implement a simple menu / settings"""
        if c == self.menu_character or c == self.exit_character:
            # Menu/exit character again -> send itself
            self.stream.send(self.tx_encoder.encode(c))
            if self.echo:
                self.console.write(c)
        elif c == '\x01':  # CTRL+A -> set encoding
            self.change_encoding()
        elif c == '\x04':                       # CTRL+D -> Toggle DTR
            try:
                self.stream.dtr = not self.stream.dtr
                sys.stderr.write('--- DTR {} ---\n'.format('active' if self.stream.dtr else 'inactive'))
            except AttributeError:
                pass
        elif c == '\x05':  # CTRL+E -> toggle local echo
            self.echo = not self.echo
            sys.stderr.write(
                '--- local echo {} ---\n'.format('active' if self.echo else 'inactive'))
        elif c == '\x06':  # CTRL+F -> edit filters
            self.change_filter()
        elif c == '\x07':  # CTRL+U -> upload file
            self.change_goahead()
        elif c in '\x08hH?':  # CTRL+H, h, H, ? -> Show help
            sys.stderr.write(self.get_help_text())
        elif c == '\x0c':  # CTRL+L -> EOL mode
            modes = list(EOL_TRANSFORMATIONS)  # keys
            eol = modes.index(self.eol) + 1
            if eol >= len(modes):
                eol = 0
            self.eol = modes[eol]
            sys.stderr.write('--- EOL: {} ---\n'.format(self.eol.upper()))
            self.update_transformations()
        elif c == '\x12':  # CTRL+R -> Toggle RTS
            try:
                self.stream.rts = not self.stream.rts
                sys.stderr.write('--- RTS {} ---\n'.format('active' if self.stream.rts else 'inactive'))
            except AttributeError:
                pass
        elif c == '\x15':  # CTRL+U -> upload file
            self.upload_file()
        elif c == '\x09':  # CTRL+I -> info
            self.dump_port_settings()
        # ~ elif c == '\x01':                       # CTRL+A -> cycle escape mode
        # ~ elif c == '\x0c':                       # CTRL+L -> cycle linefeed mode
        elif c in 'qQ':
            self.stop()  # Q -> exit app
        else:
            sys.stderr.write('--- unknown menu character {} --\n'.format(key_description(c)))

    async def preprocess(self, line):
        """Filter lines read from a file.
        """
        line = line.strip()
        if line == "" or line == "\\" or line.startswith("\\ "):
            return
        if line.startswith("#if "):
            self.layer_ += 1
            if self.layer:
                self.layer += 1
                return
            res = await self.chat(f"{line[4:]} .", timeout=True)
            if not int(res.strip()):
                self.layer = 1
            return
        if line.startswith("#ifdef "):
            self.layer_ += 1
            if self.layer:
                self.layer += 1
                return
            res = await self.chat(f"token {line[7:]} find drop 0= .", timeout=True)
            if not int(res.strip()):
                self.layer = 1
            return
        if line.startswith("#ifndef "):
            self.layer_ += 1
            if self.layer:
                self.layer += 1
                return
            res = await self.chat(f"token {line[8:]} find drop 0= .", timeout=True)
            if int(res.strip()):
                self.layer = 1
            return
        if line.startswith("#[if] "):
            self.layer_ += 1
            if self.layer:
                self.layer += 1
                return
            res = await self.chat(f"[ {line[6:]} . ]", timeout=True)
            if not int(res.strip()):
                self.layer = 1
            return
            if not int(res.strip()):
                self.layer = 1
            return
        if line == "#else":
            if self.layer <= 1:
                self.layer = 1-self.layer
            return
        if line == "#endif":
            if self.layer:
                self.layer -= 1
            if self.layer_:
                self.layer_ -= 1
            else:
                self.layer = self.layer_ = 0
                raise RuntimeError(f"{line[9:]}: '#if…' without corresponding #endif")
            return

        if self.layer:
            return

        if line == "#end":
            raise EarlyEOFError
        if line == "#echo":
            sys.stderr.write("\n")
            return
        if line.startswith("#check "):
            res = await self.chat(f"{line[7:]} .", timeout=True)
            if not int(res.strip()):
                raise RuntimeError("Check failed")
            return
        if line.startswith("#-ok "):
            try:
                res = await self.chat(f"{line[4:]} .", timeout=True)
            except TimeoutError:
                return
            else:
                raise RuntimeError("Did not fail")
        if line.startswith("#echo "):
            sys.stderr.write(line[6:]+"\n")
            return
        if line.startswith("#error "):
            raise RuntimeError(line[7:])
        if line.startswith("#delay "):
            self.goahead_delay = float(line[7:])
            return
        if line.startswith("#include "):
            layer_,self.layer_ = self.layer_,0
            await self.send_file(line[9:])
            if self.layer_:
                self.layer = self.layer_ = 0
                raise RuntimeError(f"{line[9:]}: '#if…' without corresponding #endif")
            self.layer_ = layer_
            return

        i = line.find('\\ ')
        if i > -1:
            line = line[:i].strip()
        if not line:
            return

        return line+"\n"

    async def send_file(self, filename):
        """Send a file. Runs in write thread."""
        async with await anyio.open_file(filename, 'r') as f:
            sys.stderr.write(f'--- Sending file {filename} ---\n')
            num = 0
            try:
                while True:
                    try:
                        num += 1
                        line = await f.readline()
                        if line == "":
                            break
                        line = await self.preprocess(line)
                    except EarlyEOFError as err:
                        sys.stderr.write(f'--- END {filename} : {num}\n')
                        self.layer = self.layer_ = 0
                    except EOFError as err:
                        sys.stderr.write(f'--- EOF {filename} ---\n')
                        if self.layer_:
                            self.layer = self.layer_ = 0
                            raise RuntimeError(f"{line[9:]}: '#if…' without corresponding '#endif'")
                        break
                    if not line:
                        continue
                    if self.go_ahead:
                        await self.chat(line, timeout=True)
                    else:
                        await self.stream.send(self.tx_encoder.encode(line))
                    # sys.stderr.write('.')  # Progress indicator.
            except Exception as exc:
                sys.stderr.write(f'--- in {filename} : {num}\n')
                raise

    def upload_file(self):
        """Ask user for filename and send its contents"""
        sys.stderr.write('\n--- File to upload: ')
        sys.stderr.flush()
        with self.console:
            filename = sys.stdin.readline().rstrip('\r\n')
            if filename:
                try:
                    anyio.from_thread.run(self.send_file, filename)
                    sys.stderr.write(f'\n--- File {filename} sent ---\n')
                except Exception as e:
                    if not self.develop:
                        raise
                    sys.stderr.write(f'\n--- ERROR on file {filename}: {e !r} ---\n')

    def change_filter(self):
        """change the i/o transformations"""
        sys.stderr.write('\n--- Available Filters:\n')
        sys.stderr.write(
            '\n'.join(
                '---   {:<10} = {.__doc__}'.format(k, v)
                for k, v in sorted(TRANSFORMATIONS.items())
            )
        )
        sys.stderr.write('\n--- Enter new filter name(s) [{}]: '.format(' '.join(self.filters)))
        with self.console:
            new_filters = sys.stdin.readline().lower().split()
        if new_filters:
            for f in new_filters:
                if f not in TRANSFORMATIONS:
                    sys.stderr.write('--- unknown filter: {!r}\n'.format(f))
                    break
            else:
                self.filters = new_filters
                self.update_transformations()
        sys.stderr.write('--- filters: {}\n'.format(' '.join(self.filters)))

    def change_encoding(self):
        """change encoding"""
        sys.stderr.write('\n--- Enter new encoding name [{}]: '.format(self.input_encoding))
        with self.console:
            new_encoding = sys.stdin.readline().strip()
        if new_encoding:
            try:
                codecs.lookup(new_encoding)
            except LookupError:
                sys.stderr.write('--- invalid encoding name: {}\n'.format(new_encoding))
            else:
                self.set_rx_encoding(new_encoding)
                self.set_tx_encoding(new_encoding)
        sys.stderr.write('--- serial input encoding: {}\n'.format(self.input_encoding))
        sys.stderr.write('--- serial output encoding: {}\n'.format(self.output_encoding))

    def change_goahead(self):
        """Change go-ahead sequence"""
        sys.stderr.write('\n--- Enter new go-ahead sequence: ')
        with self.console:
            new_goahead = sys.stdin.readline().strip()
        self.go_ahead= new_goahead

    def get_help_text(self):
        """return the help text"""
        # help text, starts with blank line!
        return """
--- mpy-term --- help
---
--- {exit:8} Exit program (alias {menu} Q)
--- {menu:8} Menu escape key, followed by:
--- Menu keys:
---    {menu:7} Send the menu character itself to remote
---    {exit:7} Send the exit character itself to remote
---    {info:7} Show info
---    {upload:7} Upload file (prompt will be shown)
---    {goahead:7} Go-ahead prompt for line-by-line uploads
---    {repr:7} encoding
---    {filter:7} edit filters
--- Toggles:
---    {echo:7} echo  {eol:7} EOL
""".format(
            exit=key_description(self.exit_character),
            menu=key_description(self.menu_character),
            echo=key_description('\x05'),
            info=key_description('\x09'),
            upload=key_description('\x15'),
            goahead=key_description('\x07'),
            repr=key_description('\x01'),
            filter=key_description('\x06'),
            eol=key_description('\x0c'),
        )
