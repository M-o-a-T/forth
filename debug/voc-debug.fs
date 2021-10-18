\ debug version of some word list scanners

forth only

\voc also

#require .wtag debug/voc.fs

\voc also definitions

: ~search-wl-in-ram ( c-addr u wid -- lfa|0 )
  >r dictionarystart
  begin ( c-addr u lfa )
    dup _sof_ <>
  while ( c-addr u lfa )
    dup lfa>wtag tag>wid r@ =        \ wid(lfa) = wid ?
    if
      name?   ( c-addr u lfa flag )
      if
        cr dup lfa>wtag .wtag dup .header  \ print wtag(lfa) and name(lfa)
        nip nip r> drop exit
      then
    then
    dictionarynext drop
  repeat
  ( c-addr u lfa ) 2drop r> ( c-addr wid ) 2drop 0
;


: ~search-wl-in-flash ( c-addr u wid -- lfa|0 )
  dup 0 >r >r forth-wl = if _sof_ else forth-wl @  then
   begin
    dup forth-wl >           \ tagged word ?
    if
      dup lfa>wtag tag>wid r@ =    \ wid(lfa) = wid ?
      if
        name?
        if
          r> r> drop over >r >r         \ R: wid lfa.found
          cr dup lfa>wtag .wtag dup .header  \ print wtag(lfa) and name(lfa)
        then
      then
    else ( c-addr u lfa )
      name?
      if
        r> r> drop over >r >r           \ R: wid lfa.found
        cr 9 spaces dup .header
      then
    then
    dictionarynext
  until
  ( c-addr u lfa |R: wid lfa.found ) 2drop drop  rdrop r> ( lfa|0 )
;


: ~??-wl ( c-addr u wid -- lfa|0 )
  cr ." ?> " dup .wid
  >r compiletoram?
  if
    2dup r@ ~search-wl-in-ram ?dup if nip nip r> drop exit then
  then
  r> ~search-wl-in-flash
;

: ~??-vocs ( c-addr len a-addr -- lfa|0 )
  @
  begin
   \ dup .
   >r 2dup r@ ~??-wl dup if nip nip r> drop exit then drop
   r> vocnext dup 0=
  until
  drop root-wl ~??-wl
;

: ~??-vocs-without-root ( c-addr len a-addr -- lfa|0 )
  begin
   >r 2dup r@ ~??-wl dup if nip nip r> drop exit then drop
   r> vocnext dup 0=
  until
  nip nip
;

: ~??-order ( c-addr u a-addr -- lfa|0 )
  dup >r ( c-addr u a-addr )  \ a-addr = top of the search order
  begin
    @ ( c-addr u wid|0 ) dup
  while
    >r 2dup r> dup root-wl =
    if ."  swl "
      ~??-wl
    else ."  svoc "
      ~??-vocs-without-root
    then
    dup

    if nip nip r> drop exit then drop
    r> cell+ dup >r
  repeat
  r> 2drop 2drop 0
;


: ~??-dictionary ( c-addr len -- lfa|0 )
  _sop_ @ dup context =
  if   ."  in order "  dup @ .id
    ~??-order
  else   ."  in voc "  dup @ .id ." -- ctx " context @ .id
    ~??-vocs
  then
;

: _!~csr_ ( lfa -- )
    0 _csr_ ! dup
    if ( lfa ) \ found
      ."  found: " dup .header
      ( lfa ) dup forth-wl >   \ tagged word ?
      if ( lfa )
        dup lfa>wtag ( lfa wtag ) 1 and   \ word with ctag ?
        if dup lfa>ctag ( csr ) _csr_ ! then ( lfa )
      then ( lfa )
    then ( lfa )
    drop
;

\ Process the last saved context switching request.
: _~?csr_ ( -- )
    cr _csr_ @ ." _csr_=" .  \ debugging

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

forth definitions

: ~find ( c-addr len -- xt|0 flags )
  ??
  _indic_ @
  if
    _~?csr_
     ." voc-find,_indic_ = -1 "              \ *** for debugging only
    ~??-dictionary ( lfa )
    dup _!~csr_
     ."  csr=" _csr_ @ . cr     \ *** for debugging only
  else ( lfa )
    ." voc-find,_indic_ = 0 "              \ *** for debugging only
    -1 _indic_ ! current @ ~??-wl
  then
  lfa>xt,flags
;


: ~eval  ( addr n -- )
\ given a word, find and run it.
  2dup ~find drop ?dup if
    -rot 2drop execute
  else
    space type ."  not found"
    -3 abort
  then
;

: ~voc-eval  ( addr n -- )
\ given a word in the current vocabulary, find and run it.
  1 \voc _csr_ bis! ~eval
;

: ~dovoc ( a -- )
  ."  DV: " dup hex. @ dup hex.
  (dovoc
;

0 variable ~hf

\voc definitions
: ~voc-extend ( "name" wid -- )
  ."   EXT:" dup hex. dup .wid space
  align ,
  2 wflags !
  here ( addr of names wtag ) cell+ ( lfa of name )
  ." A:" h.s
  <builds
    here dup ~hf ! ." H:" hex.
    dup ,
    ." HA " ~hf @ @ hex.
    (dovoc (def [ ' also call, ' immediate call, ]
    ." HB " ~hf @ @ hex.
  does> ~dovoc
;

root definitions

: ~voc: ( "name" -- )
  _sop_ @ context = if ." C:-" 0 else VOC-context @ dup ." C:" .wid then ~voc-extend
;

forth definitions only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
