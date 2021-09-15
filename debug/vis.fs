\ word lists, introspection etc. for vis

\voc also definitions

: .wtag ( wtag -- ) ." wtag: " hex. ;

: .ctag ( ctag -- ) ." ctag: " hex. ;

: .id ( lfa -- )
  lfa>nfa count type space
;

: .wid ( lfa|wid -- )
\ dup @ if ( wid = lfa ) .id else u. then
  dup wid? if u. else .id then
;

: ?wid ( wid1 -- wid1|wid2 )
  case
    root-wordlist  of [ (' root   literal, ]  endof
    forth-wordlist of [ (' forth  literal, ]  endof
    \voc-wl        of [ (' \voc   literal, ]  endof
  dup  \ 'endcase' drops our value, but we want to keep it
  endcase
;

: .wid ( wid -- ) ?wid .wid ;


: .header ( lfa -- )
  ." lfa: " dup hex. dup ." xt: " lfa>xt hex.    \ print lfa and xt
  ." name: " lfa>nfa count type space            \ print name
;

: show-wordlist-item ( lfa wid -- )
  >r dup forth-wordlist >=   \ tagged word ?
  if ( lfa )
    dup lfa>wtag tag>wid r@ =     \ wid(lfa) = wid
    if ( lfa )
       cr dup lfa>wtag dup 1 and if over lfa>ctag .ctag else #15 spaces then
       .wtag .header
    else
      drop
    then
  else ( lfa )
     cr #30 spaces .header
  then
  r> drop
;

: show-wordlist-item ( lfa wid -- )                        \ MM-200520
  over smudged? if show-wordlist-item else 2drop then
;

: show-wordlist-in-ram ( wid lfa -- )
  drop ( wid ) >r
  dictionarystart ( lfa )
  begin
    dup _sof_ <>
  while
    dup r@ show-wordlist-item
    dictionarynext if drop _sof_ then
  repeat
  r> 2drop ;

: show-wordlist-in-flash ( wid lfa -- )
\ List all words of wordlist wid (defined in flash) starting with word at lfa.
\ lfa must be in flash
\ forth-wordlist dictionarystart c2f-... lists all forth-wordlist words
\ forth-wordlist dup             c2f-... lists forth-wordlist words starting
\                                        with lfa(forth-wordlist)
\ wid dup                        c2f-... lists all words of wordlist wid
  swap >r  ( lfa ) ( R: wid )
  begin ( lfa )
    dup r@ show-wordlist-item
    dictionarynext
  until
  r> 2drop
;

forth definitions


\ Show all words of the wordlist wid.
: show-wordlist ( wid -- )
  dup forth-wordlist =
  if dup _sof_ else dup forth-wordlist then show-wordlist-in-flash
  compiletoram? if 0 show-wordlist-in-ram else drop then
;

\voc definitions

: ?words ( wid f -- )
\ If f = -1 do not list the mecrisp core words.
\  context @
\  swap if
   if ( wid )
    \ Show words in flash starting with FORTH-WORDLIST.
    dup forth-wordlist
  else
    \ Show words in flash starting with the first word in flash.
\   dup forth-wordlist over = if _sof_ else forth-wordlist then  \ fails MM-200102
    dup dup forth-wordlist = if _sof_ else forth-wordlist then   \ works
  then
  show-wordlist-in-flash
  dup cr ." << FLASH: " .wid
  compiletoram?
  if
    cr dup ." >> RAM:   " .wid
    0 show-wordlist-in-ram
  else
     drop
  then
  cr
;

: dashes ( +n -- ) 0 ?do [char] - emit loop ;

: \??? ( f -- )
    cr 15 dashes
    >r _sop_ @ @ ( lfa|wid )
    begin
      dup r@ ?words \ cr
      vocnext dup 0=
    until
    r> 2drop
;

forth definitions

: words ( -- ) 0 \??? cr ;


root definitions

: words ( -- ) words ;

\ Given a wid of a VOCabulary print the VOCabulary name, given a wid of a
\ wordlist print the address.

\voc definitions

: core? ( lfa -- f )
\ true if lfa is in the mecrisp core
  _sof_ @ u>= over forth-wordlist u< and
;

\ Given a words lfa print its name with allVOCabulary prefixes. If it's  a
\ word from the Stellaris core do not print the prefix forth.
: .nid ( lfa -- )
  dup core? if .id exit then
  0 swap
  begin
    dup lfa>wtag tag>wid
    dup forth-wordlist =
    if
      over dup forth-wordlist u>= swap root-wordlist u<= and if drop then -1
    else
      dup wid?
    then
  until
  over if dup forth-wordlist = if drop then then
  begin
    dup
  while
    .wid
  repeat
  drop
;

\ Given a wid of a VOCabulary print the VOCabulary name, given a wid of a
\ wordlist print the address.
: .vid ( wid -- )  \ MM-200522
  tag>wid
  dup wid? if .wid exit then
  .nid
;

root definitions
\ Display the search order and the compilation context.
: order ( -- )
  cr ." context: " context @ .wid
  voc-context @ context <> if
    cr ."     VOC: " voc-context @ .wid
  then
  _sop_ @ @ context @ <> if
    cr ."     SOP: " _sop_ @ @ .wid
  then
  _csr_ @ context @ <> if
    cr ."  switch: " _csr_ @ 1 bic ?dup if .wid else ." ---" then
    _csr_ @ 1 and if ."  switch" then
  then
  cr ."   order: " get-order 0 ?do .wid space loop
  cr ." current: " current @ .wid
  ."  >" compiletoram? if ." ram" else ." flash" then
;


\voc first definitions

: ?item ( lfa --) dup lfa>wtag 3 and 1 =
   if
      3 spaces
      dup lfa>ctag dup 1 = if drop ." sticky " else tag>wid .vid then
      $08 emit ." : " .vid
   else
     drop
   then ;


forth definitions

\ Show all context switching items which are actually defined in the dictionary.
: items ( -- )
  cr ." FLASH:"  \ show items defined in FLASH
  forth-wordlist
  begin
    dup ?item
    dictionarynext
  until
  drop
  compiletoram?
  if
    cr ."   RAM:"  \ show items defined in RAM
    dictionarystart ( lfa )
    begin
      dup _sof_ <>
    while
      dup ?item
      dictionarynext if drop _sof_ then
    repeat
    drop
  then
  space
;

\voc first definitions  decimal

: ?voc ( lfa --) dup lfa>wtag 3 and 2 =  if space .vid else drop then ;

forth definitions

\ Show all the VOCs that are actually defined in the dictionary.
: vocs ( -- )
  cr ." FLASH: "   \ show VOCs defined in FLASH
  forth-wordlist
  begin
    dup ?voc dictionarynext
  until
  drop
  compiletoram?
  if
    cr ."   RAM: "   \ show VOCs define in RAM
    dictionarystart ( lfa )
    begin
      dup _sof_ <>
    while
      dup ?voc dictionarynext if drop _sof_ then
    repeat
    drop
  then
  space
;

\voc definitions

: show-word-name ( lfa wid -- )
  >r dup forth-wordlist >=   \ tagged word ?
  if ( lfa )
    dup lfa>wtag tag>wid r@ =     \ wid(lfa) = wid
    if ( lfa )
       .id
    else
      drop
    then
  else ( lfa )
    .id
  then
  r> drop
;

: show-word-name ( lfa wid -- )                        \ MM-200520
  over smudged? if show-word-name else 2drop then
;

: show-word-in-ram ( wid lfa -- )
  drop ( wid ) >r
  dictionarystart ( lfa )
  begin
    dup _sof_ <>
  while
    dup r@ show-word-name
    dictionarynext if drop _sof_ then
  repeat
  r> 2drop ;

: show-word-in-flash ( wid lfa -- )
\ List all words of wordlist wid (defined in flash) starting with word at lfa.
\ lfa must be in flash
\ forth-wordlist dictionarystart c2f-... lists all forth-wordlist words
\ forth-wordlist dup             c2f-... lists forth-wordlist words starting
\                                        with lfa(forth-wordlist)
\ wid dup                        c2f-... lists all words of wordlist wid
  swap >r  ( lfa ) ( R: wid )
  begin ( lfa )
    dup r@ show-word-name
    dictionarynext
  until
  r> 2drop
;

forth definitions

\ Show all words of the wordlist wid.
: list-words ( wid -- )
  dup forth-wordlist =
  if dup _sof_ else dup forth-wordlist then show-word-in-flash
  compiletoram? if 0 show-word-in-ram else drop then
;

\voc definitions

: ?list ( wid f -- )
\ If f = -1 do not list the mecrisp core words.
   if ( wid )
    \ Show words in flash starting with FORTH-WORDLIST.
    dup forth-wordlist
  else
    \ Show words in flash starting with the first word in flash.
    dup dup forth-wordlist = if _sof_ else forth-wordlist then   \ works
  then
  show-word-in-flash
  dup cr ." << FLASH: " .wid
  compiletoram?
  if
    dup cr ." >> RAM:   " .wid
    0 show-word-in-ram
  else
     drop
  then
  cr
;

: \?? ( f -- )
    cr
    >r _sop_ @ @ ( lfa|wid )
    begin
      dup r@ ?list
      vocnext dup 0=
    until
    r> 2drop
;

forth definitions

: words ( -- ) 0 \?? cr ;


: list ( f -- )
  cr
  _sop_ @ @ ( lfa|wid )
  begin
    dup 0 ?list \ cr
    vocnext dup 0=
  until
  drop
;
root definitions

sticky : ??? ( -- )
  -1 \??? order ."   base: " base @ dup decimal u. base !  cr 2 spaces .s ;

sticky : ?? ( -- )
  -1 \?? order ."   base: " base @ dup decimal u. base !  cr 2 spaces .s ;

: list ( -- ) list ;

forth definitions only decimal compiletoram
