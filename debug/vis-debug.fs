\ word lists, introspection etc. for vis

\voc also definitions

\ copies of VOC stuff, with debug info


: search-wordlist-in-ram-dbg ( c-addr u wid -- lfa|0 )
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


: search-wordlist-in-flash-dbg ( c-addr u wid -- lfa|0 )
  dup 0 >r >r forth-wordlist = if _sof_ else forth-wordlist @  then
   begin
    dup forth-wordlist >           \ tagged word ?
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


: search-in-wordlist-dbg ( c-addr u wid -- lfa|0 )
  cr ." ?> " dup .wid 
  >r compiletoram?
  if
    2dup r@ search-wordlist-in-ram-dbg ?dup if nip nip r> drop exit then
  then
  r> search-wordlist-in-flash-dbg
;

: search-in-vocs-dbg ( c-addr len a-addr -- lfa|0 )
  @
  begin
   \ dup .
   >r 2dup r@ search-in-wordlist-dbg dup if nip nip r> drop exit then drop
   r> vocnext dup 0= 
  until
  drop root-wordlist search-in-wordlist-dbg
;

: search-in-vocs-without-root-dbg ( c-addr len a-addr -- lfa|0 )
  begin
   >r 2dup r@ search-in-wordlist-dbg dup if nip nip r> drop exit then drop
   r> vocnext dup 0= 
  until
  nip nip
;

: search-in-order-dbg ( c-addr u a-addr -- lfa|0 )
  dup >r ( c-addr u a-addr )  \ a-addr = top of the search order
  begin
    @ ( c-addr u wid|0 ) dup
  while
    >r 2dup r> dup root-wordlist =
    if ."  swl "
      search-in-wordlist-dbg
    else ."  svoc "
      search-in-vocs-without-root-dbg
    then
    dup 

    if nip nip r> drop exit then drop
    r> cell+ dup >r
  repeat
  r> 2drop 2drop 0
;


: search-in-dictionary-dbg ( c-addr len -- lfa|0 )
  _sop_ @ dup context =
  if   ."  in order "  dup @ .id
    search-in-order-dbg
  else   ."  in voc "  dup @ .id ." -- ctx " context @ .id
    search-in-vocs-dbg
  then
;

: _!csr-dbg_ ( lfa -- )
    0 _csr_ ! dup
    if ( lfa ) \ found
      ."  found: " dup .header 
      ( lfa ) dup forth-wordlist >   \ tagged word ?
      if ( lfa ) 
        dup lfa>wtag ( lfa wtag ) 1 and   \ word with ctag ?
        if dup lfa>ctag ( csr ) _csr_ ! then ( lfa )
      then ( lfa )
    then ( lfa )
    drop 
;

\ Process the last saved context switching request. 
: _?csr-dbg_ ( -- )
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

: find-dbg ( c-addr len -- xt|0 flags )
  ??
  _indic_ @ 
  if
    _?csr-dbg_
     ." voc-find,_indic_ = -1 "              \ *** for debugging only
    search-in-dictionary-dbg ( lfa )
    dup _!csr-dbg_
     ."  csr=" _csr_ @ . cr     \ *** for debugging only
  else ( lfa )
    ." voc-find,_indic_ = 0 "              \ *** for debugging only
    -1 _indic_ ! current @ search-in-wordlist-dbg
  then
  lfa>xt,flags
;

 
: eval-dbg  ( addr n -- )
\ given a word, find and run it.
  2dup find-dbg drop ?dup if
    -rot 2drop execute
  else
    space type ."  not found"
    -3 abort
  then
;

: voc-eval-dbg  ( addr n -- )
\ given a word in the current vocabulary, find and run it.
  1 \voc _csr_ bis! eval-dbg
;

only decimal compiletoram
