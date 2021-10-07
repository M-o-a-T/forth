==========
MoaT Forth
==========

--------------
What's a MoaT?
--------------

The MoaT system is about tooling a wired bus for home automation using
simple satellite controllers (where "simple" is anything with a Cortex M0+
or M3 and at least 64k of Flash; this is the 2020s after all).

Check `the MoaT main archive <https://github.com/M-o-a-T/moat-bus>`_
to discover more about MoaT itself.

**This** repository contains lots of non-MoaT-specific Forth code that
supports these satellites, but is (hopefully) suitable for many non-MoaT
applications.

* There's lots of peripherals to interface to. We want to control I/O
  ports. And PWM outputs. And PID control loops. And serial ports. And
  things hanging off I²C or SPI buses. And fancy bit-banged RGB lights.
  And whatever other interesting peripherals there might be.

  This repository is intended to either contain, or point to, code for
  doing so.

* Talking to a bus, multiple peripherals, etc., requires interrupts and
  multitasking and buffers and all that. Or at least, having all that as
  tested infrastructure makes life a lot easier in the long run.

  This repository contains a sizeable number of these building blocks.
  More are always welcome, provided that they fit into the rest of the
  infrastructure.

* Then there's dealing with the actual bus messages. Get a bus address,
  dispatch a message, send the reply, and all that.

  This is the part that's specific to MoaT. You can find it in the main
  archive.

* Finally, we want to test all of this without flashing real hardware. It's
  slow and tends to burn out the Flash storage if you do it excessively.
  Worse, real-world CI systems tend not to have microcontrollers hanging
  off them.

  Thus this archive supports Mecrisp-Stellaris, running on Linux, with real
  (but still cooperative) multitasking instead of a blocking console.

----------
Why Forth?
----------

Forth is cool. It's an interpreted language that can live in a few kBytes
of memory. Even with a register-optimizing compiler thrown in you can stay
below 20 kBytes.

You want to introspect *anything* on the target? Just use any
terminal program.

Forth's syntax is dead simple. Forth programs consist of words (any
characters), delimited by spaces. Yes, you can finally write code with
emojis as commands, though we'd ask you to not to. 🤔

On the minus side, Forth does take some getting used to. But so do all
these elaborate development environments (hello, Arduino …).

Another and more relevant point against Forth is the lack of libraries for
common, or not-so-common, peripherals. Well, this project's goal is to fix
this problem, at least to some extent.

In this author's not-quite-humble opinion, Forth as a language has a lot to
offer.


Show me!
++++++++

You want to dive in? Great. Read the `Get Started <doc/tut/start-here.rst>`_
document to, well, get started.

I'm not interested in some strange bus!
+++++++++++++++++++++++++++++++++++++++

No matter. Focus on the first (and maybe the second) part. Most of the code
here is intended to be perfectly useable without the MoaT stuffing.

Umm, so, well, I may be interested.
+++++++++++++++++++++++++++++++++++

Great!
Go to `the MoaT main archive <https://github.com/M-o-a-T/moat-bus>`_.

No, we don't have a running bus yet.

--------------
Final Thoughts
--------------

This author is admittedly somewhat opinionated in what should be considered
best practice when you're talking Forth (or rather, talking to something
that understands Forth).

Those are condensed in the `Coding Style <doc/meta/codingstyle.rst>`_ document
(the practical stuff), and the `Opinion <doc/meta/opinion.rst>`_ file for
the less-than-practical part.
