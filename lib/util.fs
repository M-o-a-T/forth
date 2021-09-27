\ small utility code
\ WARNING must be called with "forth only definitions" in effect


compiletoram?
compiletoflash


#if token defined find drop 0=
\ We want to use "#if defined X" instead of the above dance.

only root definitions

sticky  \ deleted by "find"
: defined   ( "token" -- flag ) token find drop 0<> ; 

sticky  \ deleted by "find" in "defined"
: undefined ( "token" -- flag ) defined not ;

#endif

forth only definitions


#if undefined unused
$deadbeef constant unused
#endif

#if \voc undefined post-def

\voc definitions
0 variable post-def
forth definitions

: ; ( -- )
\ call (and clear) the POST-DEF
  postpone ;
  \voc post-def @ ?dup if 0 \voc post-def !  execute  then
  immediate
;
#endif

#if undefined eval

: eval  ( addr n -- )
\ given a word, find and run it.
  2dup find drop ?dup if
    -rot 2drop execute
  else
    space type ."  not found"
    -3 abort
  then
;
#endif

#if undefined voc-eval  defined \voc  and
\voc also
: voc-lfa ( addr n )
\ given a word in the current vocabulary, find it.
  2dup \ remember for possible error message
  voc-context ??-vocs
  ?dup if
    -rot 2drop
  else
    space type ."  not found"
    -3 abort
  then
;
: voc-eval  ( addr n -- )
\ given a word in the current vocabulary, find and run it.
  voc-lfa lfa>xt execute
;
\voc ignore
#endif

#if undefined haligned
: haligned ( c-addr -- h-addr )
\ like "aligned" but for halfwords
  1 cells 2/ 1- tuck + swap bic
;
#endif

\ mecrisp-without-RA doesn't have these

#if-flag roll
#if undefined roll
: roll ( xu xu-1 … x0 u -- xu-1 … x0 xu )
  ?dup if  \ zero is no-op
    dup
    begin ( x* u u' )
    ?dup while
      rot >r 1-
    repeat
    ( xu u |R: xu-1 … x0 )
    begin ?dup while
      r> -rot 1-
    repeat
  then
; 
#endif

#if undefined -roll
: -roll ( xu-1 … x0 xu u -- xu xu-1 … x0 )
  ?dup if  \ zero is no-op
    dup
    begin  ( x* xu u u' )
    ?dup while
      \ 3 roll
      >r rot r> swap
      >r 1-
    repeat
    ( xu u |R: xu-1 … x0 )
    begin
    ?dup while
      r> swap 1-
    repeat
  then
;
#endif
#endif

#if undefined offset:
: offset: ( "name" n -- )
\ Simple building block for register maps and similar
  <BUILDS ,
  DOES> @ +
;
#endif

#if undefined [with]
: (with) ( str len xt -- ? )
\ run XT with the given string as parser input
  source 2>r >in @ >r \ save the interpreter state
  -rot  ( xt str len )
  setsource 0 >in !  ( xt )
  execute
  r> >in ! 2r> setsource
;

: [with] ( str len "name" -- ? )
\ Compile NAME with the given string as the next token.
  ' literal, ['] (with) call,
  immediate
;
#endif

\voc also

#if undefined init:
\voc definitions
: (init:) ( -- )
  \voc last-lfa @
  lfa>xt execute
;

forth definitions
: init: ( code … -- )
\ run this code, both immediately and after a reset
  ' (init:) post-def !
  s" %init" [with] :
;
#endif


#if undefined %init!
\voc definitions
: ?setup ( lfa -- )
\ check if this word
\ - is a buffer (flag 0x100)
\ - sets a context
\ - the context in question contains SETUP
  dup ['] forth-wl <= if drop exit then
  dup lfa>flags h@ $100 and 0= if drop exit then
  dup lfa>wtag 1 and 0= if drop exit then
  dup lfa>ctag ?dup 0= if drop exit then
  ( lfa cvoc )
  tag>wid dup voc-context !
  s" setup" rot ??-vocs dup 0= if 2drop exit then
  ( lfa SETUP )
  swap lfa>xt execute
  ( SETUP obj )
  swap lfa>xt execute
;

: %init! ( -- )
\ run everything named "%init"
\ also setup all objects
\ ignore RAM here, this is called during init

  dictionarystart begin ( addr )
    \ skip all core words
    dup lfa>nfa count s" %init" compare if
      ( addr )
      \ run it in its voc context
      dup lfa>wtag tag>wid voc-context !
      dup lfa>xt execute
    else  \ check whether the word is in a voc but not itself one
      ( addr )
      dup ?setup
    then
  ( addr )
  dictionarynext until
  drop
  [ ' forth call, ' .. call, ]
;
forth definitions
: init init %init! ;
\voc ignore

#endif

\ clean up
#if ( compiletoram-flag  -- )
compiletoram
#else
compiletoflash
#endif

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
