=======
Classes
=======

Vocabularies which can inherit from other vocabularies plus
context-switching words are, in essence, single-ancestor early-bound
classes.

The library in ``lib/class.fs`` establishes a couple of words that help
writing Forth code that looks somewhat like classes.

Caveat: Words on the stack are untyped. You need to know which class they
are and act accordingly.

Caveat: Overriding a word requires to know which class the object in
question is in. This is impossible given the previous problem, but there's
a workaround.

----------
Vocabulary
----------

There are two base classes. ``class-root`` contains the minimum vocabulary
for statically-sized classes; it's implied if you don't specify a
superclass.

If you need an additional buffer, use ``sized`` as the base class. See
below.

class: ( "name" -- )
++++++++++++++++++++

Create a class that inherits directly from ``class-root``.

<class> class: ( "name" -- )
++++++++++++++++++++++++++++

Create a class that inherits from, i.e. extends the given ``<class>``.

<class> definitions ( -- )
++++++++++++++++++++++++++

Make ``<class>`` the current compilation context. This is redundant directly
after creating the class, as we're assuming that an empty class is not
particularly interesting.

__data ( -- ivsys inherited-size )
++++++++++++++++++++++++++++++++++

Start declaring member variables.

<type> field:  ( "name" size -- size+x )
++++++++++++++++++++++++++++++++++++++++

Declare a member variable of type ``type``.

Basic variables are declared in ``lib/vars.fs``. Classes as members also
work.

here: ( "name" size -- size+x )
+++++++++++++++++++++++++++++++

Declare an untyped member variable without allocating any space. In other
words, this marks a position in your structure.

Use this for advanced data manipulation. Or not.

__seal ( ivsys size -- )
++++++++++++++++++++++++

Terminate an instance definition in the current class compilation context.

This creates the class's ``u/i`` constant, which declares how large the
space to allocate for an instance's static variables is.

size ( \*args XXX -- \*args XXX bytes )
+++++++++++++++++++++++++++++++++++++++

For classes that descend from ``sized``, this word states how much
additional room beyond the "known" variables to allocate.

If your object is parameterized using explicit stack values (as opposed to
subclassing), ``size`` may access these via the stack. It must not consume
or otherwise disturb them; that's the job of ``setup``.

setup ( object -- )
+++++++++++++++++++

Your object is initialized with zeroed-out data. If you want a different
initial state, declare a word named ``setup`` in its class.

This word is called as your object is created, or from INIT after reboot.
*Never call it yourself.* Also, don't call the superclass – that too is
done automatically.

Within your ``setup`` word, you may use ``voc-eval`` to access words from
subclasses, e.g. to read constants for parameterization. If you need to
remember their value for later, store them in one of your object's fields.

You don't need to initialize any fields to zero, that's done for you.

>setup ( object -- )
++++++++++++++++++++

If you have a complex sub-field in your object, you may need to initialize it.

To do this, call ``dup __ FIELD >setup`` from your object's ``setup`` word.

You can also use this word to re-initialize a field or an object, if that
should be necessary.

\\offset
++++++++

Sized classes contain this variable. It is filled by ``setup`` and contains
the starting offset of the variable-sized part of the object.


\__ ( -- )
++++++++++

Search the next word in the current class's context only.

;class ( -- )
+++++++++++++

Go back to the vocabulary state before the class was declared.

That state is not recorded, but inferred from the class. In particular,

* the class is removed from the current search context

* the vocabulary in which the class has been defined is set as the current
  vocabulary


-------------
Using classes
-------------

We'll start with a simple example: a class that stores a half-cell word::

	class: hint

	__data
	1 cells 2/ +
	__seal

	: @ ( a -- n ) h@ inline ;
	: ! ( n a -- ) h! inline ;
	: ? ( a -- )  h@ . ;

	;class

This looks almost boring. Usage::

	hint object: i1

	12345 i1 !
	i1 ?
	#ok i1 @ 12345 =
	i1 _addr_  hex.
	#ok i1 _addr_  @ 12345 =

Using the object does two things. It ppushes its own addres onto the stack
*and* it uses context switching so that the next word is taken from the
object's vocabulary, which is why you can use ``!`` and  ``@`` here even
though these are only half cells.

If you do any stack manipulation, though, you have to be more careful::

	hint definitions
	: mid ( h1 h2 -- h1+h2 /2 == h2+ h1-h2 /2 )
	  __ @ swap __ @ ( @h2 @h1 )
	  over - 2/ + ;

The ``__`` assumes that you're declaring the word as part of the class.
Otherwise, i.e. from code that's external to the class, use the class name
instead.

If you just want the address of an object instead of then accessing its
data, you need to reset the search context::

	forth definitions
	hint object: i2
	23456 i2 !
	i1 .. i2 mid dup .
	#ok 17900 =

(The last line is an assertion that's processed by our terminal program.)

While single-value objects are boring, you can combine them::

	class: point
	__data
	  haligned  \ no-op
	  hint field: x
	  hint field: y
	__seal

You might want to initialize things::

	: setup ( obj -- )
	  -1 over __ x !
	  -1 swap __ y !
	;
	;class

	point object: p1
	point object: p2

	#ok p1 x @ -1 =
	#ok p2 y @ -1 =

	#100 p1 x !  #200 p1 y !
	#102 p2 x !  #202 p2 y !

	#ok p1 x @ 100 =

The words ``__data`` and ``__seal`` must frame your field definition, to
ensure that the required buffer size is calculated and stored in your object.
You don't need them if your subclass doesn't contain any data of its own.

Debugging
+++++++++

It's a good idea to add a ``?`` debug word to your classes. This word
should print some detail about the object in question.

.all' ( "name" -- )
-------------------

Find the named class. Scan the dictionary. For each object of that class
(or one of its subclasses), if the word ``?`` is defined for it, it is
called; its output is followed by the word's name (preceded by the
vocabulary it's defined in, if any).

.all ( lfa -- )
---------------

As above, but use a base class whose address you looked up with ``('``.

Thus::

	\cls also
	.all' root-cls

or::

	\cls (' root-cls .all

will print a list of all your classes.

These words are located in ``debug/class.fs``.

Field alignment
+++++++++++++++

The field definition of a basic object doesn't know about its own alignment
requirements, so unfortunately you have to do that yourself.

Basic rule: write HALIGNED before the first HINT field, and ALIGNED before
the first INT field.

Whether you can get away with less strict alignment requirements and/or
whether using misaligned fields incurs a performance penalty depends on
your CPU.

Since it's easy enough to do this manually if required, and forwarding
alignment to surrounding objects is nontrivial, this library doesn't
include support for automagically fixing these issues.

The basic (empty) object is always fully aligned. A sized object currently
contains one HINT, thus starts with half-word alignment, though it's best
not to depend on that.

-------------------
Using sized classes
-------------------

Up to now, our classes had a well-defined size. However, it's often useful
to include a variable-sized data area. For instance, a ring buffer needs
static pointers to the first and last element, but also space for the
actual data.

To do that, classes can be *sized*. An additional variable area below their
fixed elements is allocated when an item is created. The size of the
fixed area is stored (by ``setup``) in the field ``\offset``.

See ``lib/ring.fs`` for an example.

Sized classes can be subclassed using a class that adds new variables.
That is no problem; ``\\offset`` is adjusted appropriately.

Sized objects vs. subclassing
+++++++++++++++++++++++++++++

If your subclass wants to add another variable-sized element, both subclass
and superclass must be written with this in mind.

The problem is that all code that dynamically looks up attributes of the
class you're creating an object of must be in (or called from) the class's
``setup`` word.

This is why our ``ring`` class uses a private ``offset`` variable. It is 
set by adding the ``size`` of the superclass to ``\offset`` (set by the
superclass's ``setup``). The redefinition of ``size`` that includes our
buffer is located after that.

Because we know that the ``sized`` base class has a size of zero,
declaration and use of the private ``offset`` variable has been commented
out in the ``ring`` code. Also, if we knew that the class will never be
subclassed with additional variable-sized elements, we could directly use
``size`` instead of requiring a separate ``elems`` constant, but we don't.

----------------------
Parameterizing objects
----------------------

``ring`` demonstrates the preferred way of declaring parameters for a
class: you create a subclass with the requisite constant, then look up the
value via ``voc-eval`` from ``setup``.

.. note

	Parameters cannot be accessed directly. They must be read via
	``voc-eval``::

	    123 constant elems
		: elems@ s" elems" voc-eval ;

	Your ``setup`` method is responsible for storing the parameter's value
	in one of the object's fields, so that any method that's called later
	can access it.

------------------------
Objects in Flash storage
------------------------

Easy. Our init word will discover all objects with ``setup`` methods and
call them all. You should not do it manually.

---------
Rationale
---------

This object system has two main deficiencies.

For one, it binds early. Way early. The only place where you can do late binding 
is during the object's construction (the ``setup`` word), and even that
requires special handling (lookup via ``voc-eval``).

The second problem is that there's no checking whatsoever. If you access a
``hint`` object without making sure that ``@`` or ``!`` are looked up from
its vocabulary instead of FORTH, interesting bugs will happen.

The author of this document expects it to be useful anyway, as it fills an
interesting niche within Forth's constrained environment. The fact that it
has zero runtime overhead (besides ``setup`` of course) is a bonus which no
late-binding system can possibly achieve.

The future will show whether that assessment is correct.

