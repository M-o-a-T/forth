\ Wordlists for Mecrisp-Stellaris
\ ------------------------------------------------------------------------------

\             Copyright (C) 2017-2020 Manfred Mahlow @ forth-ev.de

\        This is free software under the GNU General Public License v3.
\ ------------------------------------------------------------------------------
\
\ Mangled by Matthias Urlichs <matthias@urlichs.de>.
\
\ ??-* => search-in-*
\ *-wl => *-wordlist

compiletoflash      \ This extension must be compiled in flash.

hex

\ An alias for WORD in the Mecrisp Stellaris core.
: \words ( -- ) words ;


\ Three wordlists are implemented now, all as members of the forth-wl:

align 0 , here cell+ ,     here constant forth-wl
align 0 , forth-wl , here constant \voc-wl
align 0 , forth-wl , here constant root-wl


  \voc-wl ,
\ Return a wordlist identifier for a new empty wordlist.
: wordlist ( -- wid )
  align here [ here @ not literal, ] ,
;

  \voc-wl ,
\ Return true if lfa|wid is the wid of a wordlist.
: wid? ( lfa|wid -- f )
  @ [ here @ not literal, ] =
;

  \voc-wl ,
\ lfa of the first word in flash
\ "dictionarystart" changes after compileroram
dictionarystart constant _sof_

  \voc-wl ,
\ Max number of active word lists
#6 constant #vocs 

\ We need two buffers for the FORTH search order:

  \voc-wl ,
\ A buffer for the search order in compiletoflash mode.
#vocs 1+ cells buffer: c2f-context

  \voc-wl ,
\ A buffer for the search order in compiletoram mode.
#vocs 1+ cells buffer: c2r-context

  \voc-wl ,
\ Return the addr of the actual search order depending on the compile mode.
: context ( -- a-addr )
  compiletoram? if c2r-context else c2f-context then
;


  \voc-wl ,
\ Current for compiletoflash mode.
forth-wl variable c2f-current

  \voc-wl ,
\ Current for compiletoram mode.
forth-wl variable c2r-current

  \voc-wl ,
\ Current depending on the compile mode.
: current ( -- a-addr )
  compiletoram? if c2r-current else c2f-current then
;


  \voc-wl ,
\ A buffer to register a context switching request.
0 variable _csr_

  \voc-wl ,
\ A buffer for some flags in a wordlist tag (wtag).
0 variable wflags


  \voc-wl ,
\ A flag, true for searching the dictionary with context switching support,
\ false for searching the compilation context only without context switching.
-1 variable _indic_ 

  \voc-wl ,
0 variable last-lfa

  \voc-wl ,
\ Compile a wordlist tag ( wtag).
: wtag, ( -- )
  align current @ wflags @ or ,  0 wflags !  0 _indic_ !
  here last-lfa !
;

  \voc-wl ,
: lfa>flags ( a-addr1 -- a-addr2 ) cell+  inline ;

  \voc-wl ,
: lfa>nfa ( a-addr -- cstr-addr ) lfa>flags 2+  inline ;

  \voc-wl ,
: lfa>xt ( a-addr -- xt ) lfa>nfa skipstring ;

  \voc-wl ,
: lfa>wtag ( a-addr -- wtag ) 1 cells - @ ;

  \voc-wl ,
: lfa>ctag ( a-addr -- ctag ) 2 cells - @ ;

  \voc-wl ,
: tag>wid ( wtag -- wid ) 1 cells 1- bic  inline ;

  \voc-wl ,
: lfa>xt,flags ( a-addr -- xt|0 flags )
  dup if dup lfa>xt swap lfa>flags h@ else dup then
;

  \voc-wl ,
\ Return true if the word at lfa is smudged.
: smudged? ( lfa -- flag )
  cell+ h@ [ here @ FFFFFFFF = FFFF and literal, ] <>
;

\ End of Tools to display wordlists.

  \voc-wl ,
\ Return true if name of word at lfa equals c-addr,u and word is smudged.
: name? ( c-addr u lfa -- c-addr u lfa flag )
  >r 2dup r@ lfa>nfa count compare r> swap
  if  ( c-addr u lfa )  \ name = string c-addr,u
      dup smudged?
  else  \ name <> string c-addr,u
    false
  then
;


  \voc-wl ,
\ If the word with name c-addr,u is a member of wordlist wid, return its lfa.
\ Otherwise return zero.
: search-wl-in-ram ( c-addr u wid -- lfa|0 )
  >r dictionarystart 
  begin ( c-addr u lfa )
    dup _sof_ <> 
  while ( c-addr u lfa )
    dup lfa>wtag tag>wid r@ =        \ wid(lfa) = wid ?
    if
      name?   ( c-addr u lfa flag )
      if
        nip nip r> drop exit
      then
    then
    dictionarynext drop
  repeat 
  ( c-addr u lfa ) 2drop r> ( c-addr wid ) 2drop 0
;


  \voc-wl ,
\ If the word with name c-addr,u is a member of wordlist wid, return its lfa.
\ Otherwise return zero.
: search-wl-in-flash ( c-addr u wid -- lfa|0 )
  dup 0 >r >r forth-wl = if _sof_ else forth-wl @  then
   begin
    dup forth-wl >           \ tagged word ?
    if
      dup lfa>wtag tag>wid r@ =    \ wid(lfa) = wid ?
      if
        name?
        if
          r> r> drop over >r >r         \ R: wid lfa.found
          \ for debugging only :
          \ cr dup lfa>wtag .wtag dup .header  \ print wtag(lfa) and name(lfa)
          \
        then
      then
    else ( c-addr u lfa )
      name?
      if 
        r> r> drop over >r >r           \ R: wid lfa.found
        \ for debugging only :
        \ cr 9 spaces dup .header
        \
      then
    then
    dictionarynext
  until
  ( c-addr u lfa ) 2drop r> ( c-addr wid ) 2drop
  r> ( lfa|0 )
;


  \voc-wl , 
\ If the word with name c-addr,u is a member of wordlist wid, return its lfa.
\ Otherwise return zero.
: ??-wl ( c-addr u wid -- lfa|0 )
  >r compiletoram?
  if
    2dup r@ search-wl-in-ram ?dup if nip nip r> drop exit then
  then
  r> search-wl-in-flash
;


compiletoram
\ The next two words are an interim solution while compiling the vocabulary
\ support. As they're not used after this is task completed, they live in RAM.

  \voc-wl ,
\ Search the word with name c-addr,u in the search order at a-addr. If found
\ return the words lfa otherwise return 0.
: ??-order ( c-addr u a-addr -- lfa|0 )
  dup >r ( c-addr u a-addr )  \ a-addr = top of the search order
  begin
    @ ( c-addr u wid|0 ) dup
  while
    >r 2dup r> ??-wl dup
    if nip nip r> drop exit then drop
    r> cell+ dup >r
  repeat
  r> 2drop 2drop 0
;


  \voc-wl ,
\ Search the dictionary for the word with the name c-addr,u. Return xt and flags
\ if found, 0 and invalid flags otherwise.
: find-in-dict ( c-addr u -- xt|0 flags )
  context ??-order ( lfa ) lfa>xt,flags
;

compiletoflash

  \voc-wl ,
\ Return the number of wordlists (wids) in the search order.
: w/o ( -- wid1 ... widn n )
  0 context begin dup @ while swap 1+ swap cell+ repeat drop
; 

  forth-wl ,
: get-order ( -- wid1 ... widn n )
  w/o dup >r  cells context +  ( past-end |R: len )
  begin 1 cells - dup @ swap dup context = until drop r>
;

  forth-wl ,
: set-order ( wid1 ... widn n | -1 )
  dup -1 = if
    drop root-wl forth-wl 2
  else
    dup #vocs > over 1 < or if ." order overflow" cr quit then
  then

  dup >r 0 do  i cells context + !  loop
  0 r> cells context + !  \ zero terminated order
;


  forth-wl ,
: set-current ( wid -- ) current ! ;

  forth-wl ,
: get-current ( -- wid ) current @ ;


\ We have to redefine all defining words of the Mecrisp Core to make them add
\ a wordlist tag when creating a new word:
\ ------------------------------------------------------------------------------
  forth-wl ,
: : ( "name" -- ) wtag, : ;

  forth-wl set-current

: constant  wtag, constant ;
: 2constant wtag, 2constant ;

: variable  wtag, variable ;
: 2variable wtag, 2variable ;
: nvariable wtag, nvariable ;

: buffer: wtag, buffer: ;

: (create) wtag, (create) ;
: create   wtag, create ;
: <builds  wtag, <builds ;
\ ------------------------------------------------------------------------------

#include sys/abort.fs

\voc-wl set-current

: wlst-init ( -- )
  compiletoram -1 set-order
  compiletoflash -1 set-order
;

root-wl set-current

wlst-init

\ This is only needed for compiling the rest. VOC-INIT replaces the hook.
compiletoram ' find-in-dict hook-find !
compiletoflash

\ ===================================
\ Here's the actual "vocabulary" part
\ ===================================

decimal

\ \voc-wl first
get-order \voc-wl swap 1+ set-order

\voc-wl set-current

\ VOC context pointer for the compiletoflash mode.
  root-wl variable c2f-voc-context


\ VOC context pointer for the compiletoram mode.
  root-wl variable c2r-voc-context


\ VOC context pointer
: voc-context ( -- a-addr )
  compiletoram? if c2r-voc-context else c2f-voc-context then ;


\ Now that we have two context pointers ( context and voc-context ) representing
\ two search orders in two compile modes , we need a global search order pointer
\ too.


\ Search order pointer for the compiletoflash mode.
  c2f-context variable _c2f-sop_


\ Search order pointer for the compiletoram mode.
  c2r-context variable _c2r-sop_


\ Return the addr of the search oder pointer depending on the compile mode.  
: _sop_ ( -- a-addr ) compiletoram? if _c2r-sop_ else _c2f-sop_ then ; 


\ Save the context switching request of the word with address lfa.
: _!csr_ ( lfa -- )
    0 _csr_ ! dup
    if ( lfa ) \ found
      \ *** for context switching debugging only
      \ ."  found: " dup .header 
      ( lfa ) dup forth-wl >   \ tagged word ?
      if ( lfa ) 
        dup lfa>wtag ( lfa wtag ) 1 and   \ word with ctag ?
        if dup lfa>ctag ( csr ) _csr_ ! then ( lfa )
      then ( lfa )
    then ( lfa )
    drop 
;


\ Process the last saved context switching request. 
: _?csr_ ( -- )
    \ cr _csr_ @ ." _csr_=" .  \ debugging

    \ is this a postponed context switch request?
    \ if so, clear postpone flag in csr and exit
    _csr_ @ dup 1 and if  drop 1 _csr_ bic!  exit then
    ( csr|0 )
    ?dup if ( csr )  \ context switching requested
      voc-context !  0 _csr_ !  voc-context
    else
      context
    then
    ( ctx )
    _sop_ !
;


\ Given a vocs wid return the wid of the parent voc or zero if no voc was
\ inherited.
: vocnext ( wid1 -- wid2|0 )
  dup wid? if dup - else 2 cells - @ tag>wid then ;


\ Search the VOCs search order (voc-context) at a-addr for a match with the
\ string c-addr,len. If found return the address (lfa) of the dictionary entry,
\ otherwise return 0.
\ This is the same code as ??-vocs-no-root, except for the last line.
: ??-vocs ( c-addr len a-addr -- lfa|0 )
  @
  begin
   \ dup .
   >r 2dup r@ ??-wl dup if nip nip r> drop exit then drop
   r> vocnext dup 0= 
  until
  drop root-wl ??-wl
;

\ The ??-order defined above in wordlists only searches the top wordlist
\ of every search order entry. However, we need to scann all of them.
\ This is the same code as ??-vocs, except for the last line.
\ (Calling one from the other requires too much stack acrobatics
\  to be worth the trouble.)
: ??-vocs-no-root ( c-addr len a-addr -- lfa|0 )
  begin
   >r 2dup r@ ??-wl dup if nip nip r> drop exit then drop
   r> vocnext dup 0= 
  until
  nip nip
;

\ Search the word with name c-addr,u in the search order at a-addr.
\ Return the word's lfa, or zero if not found.
: ??-order ( c-addr u a-addr -- lfa|0 )
  dup >r ( c-addr u a-addr )  \ a-addr = top of the search order
  begin
    @ ( c-addr u wid|0 ) dup
  while
    >r 2dup r> dup root-wl =
    if \ ."  swl "
      ??-wl
    else \ ."  svoc "
      ??-vocs-no-root
    then
    dup 

    if nip nip r> drop exit then drop
    r> cell+ dup >r
  repeat
  r> 2drop 2drop 0
;

\ ------------------------------

\ Search the dictionary for a match with the given string. If found return the
\ lfa of the dictionary entry, otherwise return 0.
: ??-dictionary ( c-addr len -- lfa|0 )
  _sop_ @ dup context =
  if   \ ."  in order "  dup @ .id
    ??-order
  else  \ ."  in voc "  dup @ .id
    ??-vocs
  then
;

\ Search the dictionary for a match with the given string. If found return the 
\ xt and the flags of the dictionary entry, otherwise return 0 and flags.
: vocs-find ( c-addr len -- xt|0 flags )
  _indic_ @ 
  if
    _?csr_
    \  ." voc-find,_indic_ = -1 "              \ *** for debugging only
    ??-dictionary ( lfa )
    dup _!csr_
    \  ."  csr=" _csr_ @ . cr     \ *** for debugging only
  else ( lfa )
    \ ." voc-find,_indic_ = 0 "              \ *** for debugging only
    -1 _indic_ ! current @ ??-wl
  then
  lfa>xt,flags
;

: (dovoc ( wid -- )
  _csr_ ! _?csr_ 1 _csr_ !
;

: dovoc ( a -- )
  @ (dovoc
;

root-wl set-current   \ Some tools needed in VOC contexts

\ Switch back from a VOC search order (voc-context) to the FORTH search order.
: .. ( -- )
  0 _csr_ !  _?csr_  immediate
;

\voc-wl set-current

: (def ( -- )
  _sop_ @ @ set-current ;

root-wl set-current   \ Some tools needed in VOC contexts

: definitions ( -- ) 
  (def [ ' .. call, ] immediate ;

: defs ( -- ) 
\ I'm typing my fingers off with "definitions"
  (def [ ' .. call, ] immediate ;


\ Make the current compilation context the new search context.
: @voc ( -- )
  get-current (dovoc immediate
;

2 wflags !
: root ( -- )   root-wl   (dovoc  immediate ;
2 wflags !
: forth ( -- )  forth-wl  (dovoc  immediate ;

forth-wl set-current

2 wflags !
: \voc ( -- ) \voc-wl (dovoc  immediate ;


\voc-wl set-current


: vocs-quit ( -- )
  0 _csr_ !  context _sop_ !
  \ ." reset "  \ only for debugging
  [ hook-quit @ literal, ] execute
;

: (ign ( voc -- )
\ remove this vocabulary from the search list
  dup root-wl = if drop exit then  \ root cannot be removed
  >r get-order r>
  ( voc… n ign )
  over 0 do
    i 2 + pick
    ( voc… n ign voc-N )
    over =
    if
      drop  ( voc… n )
      i 1+ roll drop  ( voc-1… n )
      1- set-order
      unloop exit
    then
  loop
  \ not found. Oh well. Just clean up.
  drop 0 do drop loop
;

: (also) ( voc -- )
\ Add VOC to the search order, ensuring that it's not in there twice.
  dup root-wl = if drop exit then  \ root can't be added
  dup >r (ign
  get-order dup #vocs = if ."  ? search order overflow " abort then
  r> swap 1+ set-order ;


root-wl set-current

: first ( -- )
\ in theory, overwrite the top of the permanent search order with the top
\ wid of the current temporary search order.
\ TODO: FOO FIRST  BAR FIRST  currently does not drop FOO.
\ Use FOO IGNORE to ensure that this happens.
  voc-context @ (also) immediate ;

: only ( -- )
\ use only the base vocabulary (forth+root)
\ plus whatever is current
  voc-context @ root-wl = ( new root )
  if root-wl 1 set-order exit then
  root-wl forth-wl dup voc-context @ = if
    2
  else
    voc-context @ 3 
  then set-order
  immediate ;

: also ( -- ) 
\ add the current temp vocabulary to the search list
  voc-context @ (also) immediate ;

: previous ( -- )
\ remove the last-added vocabulary from the search list
  get-order dup 1 = if ."  search order underflow" abort then
  over root-wl = if
    r> swap >r
  then  \ if the root is on top, drop the voc below it
  nip 1- set-order immediate ;

: ignore ( -- )
\ remove the current temp vocabulary from the search list
  _sop_ @ @ (ign immediate ;

\voc-wl set-current

\ Create a VOC that extends the VOC wid (inherits from VOC wid).
: voc-extend ( "name" wid -- )
  \ compile the VOCs itag ( the wid of the inherited VOC )
  align ,
  \ create the VOC as an immediate word
  2 wflags !  \ set voc-flag in wtag
  here ( addr of names wtag ) cell+ ( lfa of name )
  <builds
    \ we want to:
    \ - store the address so "dovoc" can get it
    \ - set the (silent) VOC context
    \ - set definitions
    \ - add the word to the search order
    dup ,
    dup voc-context !
    dup (also)
    set-current 
    [ ' .. call, ' immediate call, ] 
    \ mark the new word as immediate
  does>
    dovoc 
;

root-wl set-current

\ Make the next created word a sticky one.
: sticky ( -- ) align 1 , 1 wflags ! ;


root-wl set-current   \ Some tools needed in VOC contexts

\ Create a VOC that extends (inherits from) the actual VOC context.
: voc: ( "name" -- )
  _sop_ @ context = if 0 else VOC-context @ then voc-extend
;


\ restore basic system state


\ \voc-wl also

get-order nip \voc-wl swap set-order


\ Make the next created word a context switching one (assign a ctag).
\ Usage: <voc> item <defining word> ...   i.e.:  123 item variable i1
: item ( -- )
  \ compile the actual VOCs wid as the next created words ctag
    align voc-context @ ,
  \ set bit 0 of the wflags in the next created words wtag
    1 wflags !
;

root-wl set-current   \ Some tools needed in VOC contexts

\ Print the data stack and stay in the current context.
sticky
: .s ( -- ) .s ;


\voc-wl set-current

\ Initialize Mecrisp with wordlist and voc extension.
: voc-init ( -- )
  wlst-init  ['] vocs-quit hook-quit !  ['] vocs-find hook-find !
;

\ \voc-wl also
get-order nip \voc-wl swap set-order

forth-wl set-current
: init ( -- )
#[if] token init find drop
  init
#endif
  voc-init
;

\ forth-wl first

forth-wl set-current

voc-init  \ now vocs can be used.

get-order nip forth-wl swap set-order

compiletoflash
\ this is necessary to clean ecerything up.
\ TODO this is a bug; figure out why the *censored* this is.

root definitions  \voc only

: (') ( str len -- lfa )
  2dup ??-dictionary ?dup if -rot 2drop exit then
  ."   " type ."  not found." abort
;  

: (' ( "name" -- lfa )
  token (')
;

\voc definitions

: ?wid ( wid1 -- wid1|wid2 )
\ replaces root/forth-wl with root/forth
  case
    root-wl  of [ (' root   literal, ]  endof
    forth-wl of [ (' forth  literal, ]  endof
    \voc-wl        of [ (' \voc   literal, ]  endof
  dup  \ 'endcase' drops our value, but we want to keep it
  endcase
;

forth definitions

\ Those words cannot be in root because normally the forth context is used
\ first, so the old definition would be found.

: ' ( "name" -- xt ) (' lfa>xt ;

: ['] ( "name" -- ) ' literal, immediate ;

\ postpone needs to be redefined because the core word uses the internal
\ tick which doesn't go through HOOK-FIND.

: postpone ( "name" -- )
  (' lfa>xt,flags $10 and if
    call,
  else
    literal, ['] call, call,
  then
  immediate
;

root definitions

\ These words need to be in root, so that they work
\ when called from within a context switch.
: postpone postpone postpone immediate ;
: ' ' ;
: ['] postpone ['] immediate ;

forth definitions

: forgetram 
  compiletoram? compiletoram

  [ ' forth call, ' only call, ' definitions call, ' .. call, ]
  forgetram

  not if compiletoflash then
;


root definitions

: postpone ( "name" -- ) postpone postpone immediate ;


forth definitions only decimal compiletoram

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
