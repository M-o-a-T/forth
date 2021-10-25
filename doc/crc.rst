===============
CRC calculation
===============

CRCs are annoying. They're also somewhat difficult to wrap your head
around, so we'll start with a small primer.

++++++++++++
Introduction
++++++++++++

A CRC works like this. You start with some value (typically zero or -1).
For each bit of your input, you exclusive-or that bit with the lowest bit
of your value. Then you shift the value one bit to the right. If the bit
that you just shifted out is set, you exclusive-or the value with your
"CRC polynomial".

Continue until you've done all your bits.

Variations include
* which polynomial to use
* which value to start with
* whether to shift the whole thing left or right
* whether to reverse the input bits
* whether to invert the result

The interesting part is that if you do this right and feed the resulting
CRC into this algorithm, all those XORs cancel each other out and you end
up with the start value. Or the inverted start value. (This mattered when
those CRCs were implemented in real hardware with limited transistor
counts; today, not so much.)

A more practical reason why CRCs are nice is that since all of this is
governed by CRCs, you can do these steps in any order. In particular, you
can shift more than one bit to the right (or left) and pre-calculate the
number to XOR into your value, which speeds up the whole thing. As
pre-calculating an 8-bit table is equivalent to pre-calculating a 4-bit
table and doing the above thing twice, you can decide on a suitable
trade-off between speed and Flash memory space.

A "CRC polynomial" is just a pre-determined number with a couple of bits
set. The name results from the maths that's typically used to arrive at the
concept.

This polynomial should not be chosen randomly. See `this paper
<http://users.ece.cmu.edu/~koopman/roses/dsn04/koopman04_crc_poly_embedded.pdf>`_
for a comprehensive analysis of which polynomials are particuarly
suited for various combination of message length, number of bit errors that
shall be recognized, and other relevant factors.

Limitations
+++++++++++

This library does not bother with inverting or reversing anything.
It always shifts to the right because that's way easier: you simply test
bit zero and otherwise ignore the width of the polynomial.

For compatibility with standard polynomials it might be necessary to
reverse the polynomial and/or the result.

This library does not use, or indeed know about, hardware CRC calculators.

+++++
Words
+++++

The parameters are

* poly: the polynomial to use
* depth: how many bits to process

There is no parameter for the actual width of the CRC (i.e. CRC-8 vs.
CRC-15 vs. CRC-24) because we don't need it: as the CRC is always shifted
to the right, excess bits remove themselves. There are however variants for
different word sized for the CRC tables, as to not waste too much space.

Generic
+++++++

crc ( crc^val poly depth -- crc' )
----------------------------------

Our basic CRC calculator. For each step, first XOR ``depth`` bits into the
previous CRC, then call this word.

This word's runtime is proportional to ``depth``.

_crc ( depth poly crc^val -- depth poly crc' )
----------------------------------------------

As above, but keep the CRC generating parameters on the stack.

CRC tables
++++++++++

Since full cells for 8- or 16-bit-wide tables are a waste of space, each
table setup word is implemented three times.

We don't bother with smaller subdivisions (e.g. three nibbles for CRC-9 to
CRC-12, or three bytes for CRC-17 to CRC-24) because the overhead accessing
the table values would be prohibitive and it's be faster to do two
lookups instead, if you need the space.

Likewise, shifting multiple bits and masking off the CRC etc. required for
multi-bit operation carries some overhead; this means that you should use
``crc`` instead of look-up tables if your inputs are less than ~4 bits wide.


_t8: ( depth poly "name" -- )
-----------------------------

Create a lookup table for polynomials up to 8 bits' width.

This allocates a table of 2^depth bytes.

crc8 ( crc byte table -- crc' )
-------------------------------

Given an "old" CRC value, a word to mix into the CRC, and the polynomial
table created by ``_t8``, return a new CRC value.

_t16 / _t32
-----------

Like ``_t8`` but for polynomials of up to 16 / 32 bits.

crc16 / crc32
-------------

Like ``crc8`` but for polynomials of up to 16 / 32 bits.

