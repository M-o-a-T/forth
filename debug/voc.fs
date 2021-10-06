\ Print word names (including their vocabulary paths), show voc state, etc

\voc also definitions

#if defined .idd
forth only definitions
#end
#endif

: .wtag ( wtag -- ) ." wtag: " hex. ;

: .ctag ( ctag -- ) ." ctag: " hex. ;

: .id ( lfa -- )
  ?wid lfa>nfa ctype space
;

: .wid ( lfa|wid -- )
  dup wid? if u. else .id then
;

: (idd) ( lfa -- )
  dup forth-wl > if
    dup lfa>wtag tag>wid
    ?dup if
      recurse
    then
    .id
  else
    drop
  then
;

: .idd ( lfa -- )
  dup forth-wl > if
    (idd)
  else
    .id
  then
;

: .wid ( wid -- ) ?wid .wid ;


: .header ( lfa -- )
  ." lfa: " dup hex. dup ." xt: " lfa>xt hex.    \ print lfa and xt
  ." name: " lfa>nfa count type space            \ print name
;

: ramdictstart ( -- lfa )
\ always returns RAM dictionary start
  compiletoram? compiletoram
  dictionarystart
  swap not if compiletoflash then
;

: show-wl-item ( lfa wid -- )
  >r dup forth-wl >=   \ tagged word ?
  if ( lfa )
    dup lfa>wtag tag>wid r@ =     \ wid(lfa) = wid
    if ( lfa )
       cr dup lfa>wtag dup 3 and if over lfa>ctag .ctag else #15 spaces then
       .wtag .header
    else
      drop
    then
  else ( lfa )
     cr #30 spaces .header
  then
  r> drop
;

: show-wl-item ( lfa wid -- )
  over smudged? if show-wl-item else 2drop then
;

forth definitions

: .show
    dup lfa>wtag tag>wid show-wl-item
;

root definitions

sticky
: show ( "name" )
  token ??-dictionary
  ?dup if
    .show
  else
    ." not found"
  then
;

\voc  definitions

: show-wl-in-ram ( wid lfa -- )
  drop ( wid ) >r
  ramdictstart ( lfa )
  begin
    dup _sof_ <>
  while
    dup r@ show-wl-item
    dictionarynext if drop _sof_ then
  repeat
  r> 2drop ;

: show-wl-in-flash ( wid lfa -- )
\ List all words of wordlist wid (defined in flash) starting with word at lfa.
\ lfa must be in flash
\ forth-wl dictionarystart c2f-... lists all forth-wl words
\ forth-wl dup             c2f-... lists forth-wl words starting
\                                        with lfa(forth-wl)
\ wid dup                        c2f-... lists all words of wordlist wid
  swap >r  ( lfa ) ( R: wid )
  begin ( lfa )
    dup r@ show-wl-item
    dictionarynext
  until
  r> 2drop
;

forth definitions


\ Show all words of the wordlist wid.
: show-wl ( wid -- )
  dup forth-wl =
  if dup _sof_ else dup forth-wl then show-wl-in-flash
  0 show-wl-in-ram
;

\voc definitions

: ?words ( wid f -- )
\ If f = -1 do not list the mecrisp core words.
\  context @
\  swap if
   if ( wid )
    \ Show words in flash starting with FORTH-wl.
    dup forth-wl
  else
    \ Show words in flash starting with the first word in flash.
    dup  dup forth-wl = if _sof_ else forth-wl then
  then
  over cr .idd cr ." FLASH: "
  show-wl-in-flash
  dup cr
  ." RAM:   "
  0 show-wl-in-ram
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
  _sof_ @ u>= over forth-wl u< and
;

\ Given a words lfa print its name with allVOCabulary prefixes. If it's  a
\ word from the Stellaris core do not print the prefix forth.
: .nid ( lfa -- )
  dup core? if .id exit then
  0 swap
  begin
    dup lfa>wtag tag>wid
    dup forth-wl =
    if
      over dup forth-wl u>= swap root-wl u<= and if drop then -1
    else
      dup wid?
    then
  until
  over if dup forth-wl = if drop then then
  begin
    dup
  while
    .wid
  repeat
  drop
;

\ Given a wid of a VOCabulary print the VOCabulary name, given a wid of a
\ wordlist print the address.
: .vid ( wid -- )
  tag>wid
  dup wid? if .wid exit then
  .nid
;

root definitions
\ Display the search order and the compilation context.
: order ( -- )
  voc-context @ context <> if
    cr ."     VOC: " voc-context @ .wid
    _sop_ @ context <> if
      ." :SOP: "
    then
  then
\  _csr_ @ context @ <> if
\    cr ."  switch: " _csr_ @ 1 bic ?dup if .wid else ." ---" then
\    _csr_ @ 1 and if ."  switch" then
\  then
  cr ."  search: " get-order 0 ?do .wid space loop
  cr ." compile: " current @ .wid
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
  forth-wl
  begin
    dup ?item
    dictionarynext
  until
  drop
  cr ."   RAM:"  \ show items defined in RAM
  ramdictstart
  begin
    dup _sof_ <>
  while
    dup ?item
    dictionarynext if drop _sof_ then
  repeat
  drop
  space
;

\voc first definitions  decimal

: ?voc ( lfa --) dup lfa>wtag 3 and 2 =  if space .vid else drop then ;

forth definitions

\ Show all the VOCs that are actually defined in the dictionary.
: vocs ( -- )
  cr ." FLASH: "   \ show VOCs defined in FLASH
  forth-wl
  begin
    dup ?voc dictionarynext
  until
  drop
  cr ."   RAM: "   \ show VOCs define in RAM
  ramdictstart ( lfa )
  begin
    dup _sof_ <>
  while
    dup ?voc dictionarynext if drop _sof_ then
  repeat
  drop
  space
;

\voc definitions

: (swn) ( lfa wid -- )
  >r dup forth-wl >=   \ tagged word ?
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

: show-word-name ( lfa wid -- )
  over smudged? if (swn) else 2drop then
;

: show-word-in-ram ( wid lfa -- )
  drop ( wid ) >r
  ramdictstart ( lfa )
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
\ forth-wl dictionarystart c2f-... lists all forth-wl words
\ forth-wl dup             c2f-... lists forth-wl words starting
\                                        with lfa(forth-wl)
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
  dup forth-wl =
  if dup _sof_ else dup forth-wl then show-word-in-flash
  compiletoram? if 0 show-word-in-ram else drop then
;

\voc definitions

: ?list ( wid f -- )
\ If f = -1 do not list the mecrisp core words.
   if ( wid )
    \ Show words in flash starting with FORTH-wl.
    dup forth-wl
  else
    \ Show words in flash starting with the first word in flash.
    dup dup forth-wl = if _sof_ else forth-wl then
  then
  over cr .idd cr ." FLASH: "
  show-word-in-flash
  dup cr ." RAM:   " 
  0 show-word-in-ram
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

\ If multitasking, the standard .s won't work in subtasks
sticky : ??? ( -- )
  -1 \??? order ."   base:" base @ dup decimal u. base !  cr
#if-flag multi
#else
  2 spaces .s
#endif
;

sticky : ?? ( -- )
  -1 \?? order ."   base:" base @ dup decimal u. base !  cr
#if-flag multi
#else
  2 spaces .s
#endif
;

: list ( -- ) list ;

forth definitions only decimal

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
