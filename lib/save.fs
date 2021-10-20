\ Save a copy of the current Forth core with all contents of the user flash

\ -----------------------------------------------------------------------------
\ useful system calls
\ -----------------------------------------------------------------------------

\ : syscall ( r0 r1 r2 r3 r4 r5 r6 Syscall# -- r0 )

\ int open(const char *pathname, int flags, mode_t mode);

forth only definitions

#if undefined syscall
#end
#endif

#if undefined sys
compiletoflash
#require lib/syscall.fs
#endif

#if defined \file
#end
#endif

compiletoram
voc: \file
sys also

\ -----------------------------------------------------------------------------
\ reading and writing ELF headers
\ -----------------------------------------------------------------------------

\ retrieve program headers
: elfhdr>phdrs ( elfhdr -- phdrs )
  dup $1c + @ + ;

\ update program header file size
: p_filesz! ( phdr filesz -- )
  swap $10 + ! ;

\ retrieve section headers
: elfhdr>shdrs ( elfhdr -- shdrs )
  dup $20 + @ + ;

\ retrieve section header size
: elfhdr>shentsize ( elfhdr -- entsize )
  $2e + h@ ;

\ retrieve the desired section header
: elfhdr>shdr ( elfhdr i -- shdr )
  >r dup elfhdr>shdrs swap elfhdr>shentsize r> * + ;

\ adjust section end
: sh_end! ( shdr end -- )
  over $0c + @ -         \ compute sh_size from sh_addr and end
  swap $14 + ! ;         \ update sh_size

\ adjust section base address in image and memory
: sh_addr! ( shdr addr -- )
  over $0c + @ - >r         \ compute adjustment
  r@ over $0c + +!          \ adjust sh_addr
  r@ over $10 + +!          \ adjust sh_offset
  $14 + r> negate swap +! ; \ adjust sh_size

\ ELF section numbers (see elfheader.s)
2 constant #mecrisp
3 constant #userdictionary

\ adjust our own ELF header according to here
: adjustelf ( -- )
  incipit elfhdr>phdrs here incipit - p_filesz!
  incipit #mecrisp elfhdr>shdr here sh_end!
  incipit #userdictionary elfhdr>shdr here sh_addr! ;

\ -----------------------------------------------------------------------------
\ saving the program image back to disk
\ -----------------------------------------------------------------------------

: save ( addr len -- ) \ Save a copy of the whole Forth core with flash dictionary contents

  compiletoram? compiletoflash >r

  adjustelf
  compiletoram
  S_ IRUSR S_ IWUSR S_ IXUSR or or call creat \ open new program image
  compiletoflash
  dup incipit here incipit - call write drop
  call close

  r> if compiletoram then
;

: save" ( -- ) [char] " parse save ;

\ -----------------------------------------------------------------------------

forth only definitions

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
