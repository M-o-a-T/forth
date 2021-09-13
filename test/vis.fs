\ word lists etc. for vis

compiletoram
inside also definitions

: .wtag ( wtag -- ) ." wtag: " hex. ;
           
: .ctag ( ctag -- ) ." ctag: " hex. ;

: show-wordlist-item ( lfa wid -- )
  >r dup forth-wordlist >=   \ tagged word ?
  if ( lfa )
    dup lfa>wtag tag>wid r@ =     \ wid(lfa) = wid
    if ( lfa )
     \ long version
       cr dup lfa>wtag dup 1 and if over lfa>ctag .ctag else #15 spaces then
       .wtag .header
     \ short
       \ .id space
    else
      drop
    then
  else ( lfa )
   \ long
     cr #30 spaces .header
   \ short
     \ .id space
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
  
  
  inside-wordlist ,
\ : show-wordlist-in-flash ( wid lfa -- )
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

inside definitions

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
;

: dashes ( +n -- ) 0 ?do [char] - emit loop ;

: \?? ( f -- )                
    cr 15 dashes              
    >r _sop_ @ @ ( lfa|wid )  
    begin                     
      dup r@ ?words \ cr      
      vocnext dup 0=          
    until                     
    r> 2drop                  
;                             

forth definitions

: words ( -- ) 0 \?? cr ;
 

root definitions
 
: words ( -- ) words ;      
                              
\ Given a wid of a VOCabulary print the VOCabulary name, given a wid of a
\ wordlist print the address.

inside definitions
: .vid ( wid -- )  \ MM-200522
  tag>wid
  dup wid? if .wid exit then
  .nid 
;

root definitions
\ Display the search order and the compilation context.
: order ( -- )
  cr ." context: " get-order 0 ?do .wid space loop
  cr ." current: " current @ .wid
  ."  compileto" compiletoram? if ." ram" else ." flash" then
;

sticky : ?? ( -- )          
  -1 \?? order ."   base: " base @ dup decimal u. base !  cr 2 spaces .s ;
 

inside first definitions
  
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

inside first definitions  decimal
  
: ?voc ( lfa --) dup lfa>wtag 3 and 2 =  if space space .vid else drop then ;
    
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

forth first
