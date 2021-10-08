===========
Coding Woes
===========

Where's the code?
=================

Today (this is written in 2021), if I want to update any reasonably complex
piece of software, I do this::

	$ git pull  ## not necessarily from github
	$ make test
	$ make install

… and I am done. Assuming there are no conflicts and the tests succeed, of
course.

However, it seems that many Forth programmers still live in the last
century. Code on Sourceforge? In a bunch of badly-versioned tar or zip
files, without the benefit of a versioned source archive, much less a
distributed one?

Please don't complain that Forth is all but invisible on today's Internet,
and *at the same time* refuse to publish it there.

Testing
=======

Yeah, I know, tests are annoying to write, and all that.

The problem is that not writing test for your code takes even longer. Ever
typed the same series of commands at your machine twents times before the
thing finally did what it's supposed to do? How about saving nineteen of
those, and write them into some file instead which you then simply execute?

So get off that stupid habit and write tests first. Then add a test for
anything that you notice going wrong. Don't just fix the problem – make
sure it doesn't come back.

This is worse on embedded things. You want to hide a LED controller behind
the bathroom mirror? Fine, but let's please make sure that our
over-the-wire upgrade *cannot* fail and that the thing will *not* hang due
to a problem we didn't (or worse, cannot) test for.

Licensing
=========

Call me an open source radical, but I have a rather strong opinion about
the licensing mess we find ourselves in.

To put it simply: if you publish your code under a MIT-ish license, then
you're perfectly OK with people who play with it, enhance it, profit from
it etc., but don't share *their* work with the community they took it from.

Guess what? I don't like the attitude of people who do that. So I don't
license my code that way. Easy decision.

If your decision is different, well, then you can write your own code.

Copyright assignment
++++++++++++++++++++

Doesn't work, either practically (plenty of people who would otherwise
contribute won't sign such a thing) or legally (plenty of countries simply
don't allow your copyright to be transferred to somebody else – you create
it, you own it).

Rants aside …
=============

The author of this document would be the last person to acknowledge that
many statements in this documentation are more-or-less substantiated
assertions how things are done, should be done, might be done better, or
better shouldn't be done at all.

He does hope that these statements do not alienate those whose code is
included in, or referred to from, these files and documents. Likewise, the
mangling of external code that inevitably accompanies attempts to unify
multiple disparate styles into one hopefully-somewhat-coherent whole is
certain to alienate some authors of said external code.

However, projects need focus. A common style and a common way of doing
things have been proven to be more important to ultimately reach a goal
because you need to spend less time and mental capacity to decipher what
the *censored* this other author could possibly have meant as she indented
her code by a mixture of ttw and five spaces.

Even if you need to get used to it.

