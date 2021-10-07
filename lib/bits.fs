forth definitions only

#if defined bits
#end
#endif

voc: bits

: bit ( n -- mask ) 1 swap lshift  1-foldable ;  \ turn a bit position into a bitmask

voc: _reg
: port: ( "name" a -- ) item constant ;
previous definitions

voc: _rg  \ common register-based words

: <% 0 0
  postpone first
  \ XXX This is evil: 0-foldable words are always run at
  \ compile time, and we depend FIRST adding our vocabulary.
  \ Workaround for other Forth systems would be to make it immediate
  \ and LITERAL, two zeroes instead. We don't do that here because
  \ constant folding wouldn't work then.
  0-foldable ;
previous definitions

_rg voc: &rg
: @ @ inline ;
: ! ! inline ;
: m! ( addr bits mask )
  #2 pick @ over not and ( addr bits mask oval )
  -rot and or swap !
;
: %>! ( addr bits mask )
  __ m!  postpone previous
;

previous definitions

_rg voc: &rg16
: @ h@ inline ;
: ! h! inline ;
: m! ( addr bits mask )
  2 pick @ over not and ( addr bits mask oval )
  -rot and or swap h!
;
: %>! ( addr bits mask )
  __ m!  postpone previous
;
previous definitions

_rg voc: &rg8
: @ c@ inline ;
: ! c! inline ;
: m! ( addr bits mask )
  2 pick @ over not and ( addr bits mask oval )
  -rot and or swap c!
;
: %>! ( addr bits mask )
  __ m!  postpone previous
;
previous definitions


voc: _bi
: +% ( bits mask pos )
  bit  rot over or ( mask val val+ )
  -rot or
  3-foldable
;
: -% ( bits mask pos )
  bit  rot over bic ( mask val val- )
  -rot or
  3-foldable
;
: *% ( bits mask val pos )
  swap if __ +% else __ -% then 
  4-foldable
;
previous definitions

_bi voc: &bi
: @ bit  swap bit@ ;
: ! ( flg addr shift )
  bit  swap rot
  if bis! else bic! then ;
: +! ( addr shift )
  bit  swap bis! ;
: -! ( addr shift )
  bit  swap bic! ;

previous definitions

_bi voc: &bi16
: @ bit  swap hbit@ ;
: ! ( flg addr shift )
  bit  swap rot
  if hbis! else hbic! then ;
: +! ( addr shift )
  bit  swap hbis! ;
: -! ( addr shift )
  bit  swap hbic! ;
previous definitions

_bi voc: &bi8
: @ bit  swap cbit@ ;
: ! ( flg addr shift )
  bit  swap rot
  if cbis! else cbic! then ;
: +! ( addr shift )
  bit  swap cbis! ;
: -! ( addr shift )
  bit  swap cbic! ;
previous definitions

voc: _bf
: _spl ( widthshift -- shift mask )
  dup $1f and swap
  #5 rshift bit 1-
  1-foldable ;

: *% ( bits mask val shiftmask -- bits mask ) 
  __ _spl ( bits mask val shift mask )
  >r >r r@ lshift ( bits mask val< |R: mask shift ) 
  rot or swap ( nbits mask )
  r> r> swap lshift  ( nbits mask mask< )
  or
  4-foldable ;

previous definitions

_bf voc: &bf
: @ ( addr shiftmask )
  __ _spl
  rot @ and swap rshift ;

: ! ( val addr shiftmask )
  __ _spl ( val addr shiftmask )
  \ first apply the shift to both mask and value
  >r >r swap r@ lshift
  r> r> swap lshift  ( addr val< mask< )
  &rg m!
;
previous definitions

_bf voc: &bf16
: @ ( addr shiftmask )
  __ _spl
  rot h@ and swap rshift ;

: ! ( val addr shiftmask )
  __ _spl
  \ first apply the shift to both mask and value
  >r >r swap r@ lshift
  r> r> swap lshift  ( addr val< mask< )
  &rg16 m!
;
previous definitions

_bf voc: &bf8
: @ ( addr shiftmask )
  __ _spl
  rot c@ and swap rshift ;

: ! ( val addr shiftmask )
  __ _spl
  \ first apply the shift to both mask and value
  >r >r swap r@ lshift
  r> r> swap lshift  ( addr val< mask< )
  &rg8 m!
;


forth definitions only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
