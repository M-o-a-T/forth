#ifndef defined
: defined token ( -- addr ) find drop 0<> ; 
#endif
#ifdef dly
#else
: dly 3000000 0 do loop ;
#endif

#if defined foo not
12345
#else
54321
#endif
.
#ifndef defined
4321
#else
1234
#endif
.
#ifdef defined
123
#else
321
#endif
.

dly
42 .

