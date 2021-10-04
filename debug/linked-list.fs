
#require .word-off lib/crash.fs

\voc \d-list definitions also

: ? ( list -- )
  ." DL:" \ dup dup hex. .word-off
  dup ( list list )
  2dup __ next @ .. = if
    ." same" 2drop exit
  then
  __ next @ swap __ prev @
  ( next prev )
#if-flag debug
  over poisoned = over poisoned = or if
    2drop ." unlinked"
    exit
  then
#endif
  dup ." <" dup hex. .word-off
  ." >"
  over = if
    ." same" drop
  else
    dup hex. .word-off
  then
;

previous
