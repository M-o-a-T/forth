
#require .word-off lib/crash.fs

\voc \d-list definitions also

: ? ( list -- )
  ." DL:" \ dup dup hex. .word-off
  dup
  ." <" __ prev @ dup hex. .word-off
  ." >" __ next @ dup hex. .word-off
  cr
;

previous
