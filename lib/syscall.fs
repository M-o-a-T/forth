#if defined syscall

forth only definitions

voc: sys

voc: err
  1 constant EPERM           \ Operation not permitted
  2 constant ENOENT          \ No such file or directory
  3 constant ESRCH           \ No such process
  4 constant EINTR           \ Interrupted system call
  5 constant EIO             \ I/O error
  6 constant ENXIO           \ No such device or address
  7 constant E2BIG           \ Argument list too long
  8 constant ENOEXEC         \ Exec format error
  9 constant EBADF           \ Bad file number
 10 constant ECHILD          \ No child processes
 11 constant EAGAIN          \ Try again
 EAGAIN constant EWOULDBLOCK \ Operation would block
 12 constant ENOMEM          \ Out of memory
 13 constant EACCES          \ Permission denied
 14 constant EFAULT          \ Bad address
 15 constant ENOTBLK         \ Block device required
 16 constant EBUSY           \ Device or resource busy
 17 constant EEXIST          \ File exists
 18 constant EXDEV           \ Cross-device link
 19 constant ENODEV          \ No such device
 20 constant ENOTDIR         \ Not a directory
 21 constant EISDIR          \ Is a directory
 22 constant EINVAL          \ Invalid argument
 23 constant ENFILE          \ File table overflow
 24 constant EMFILE          \ Too many open files
 25 constant ENOTTY          \ Not a typewriter
 26 constant ETXTBSY         \ Text file busy
 27 constant EFBIG           \ File too large
 28 constant ENOSPC          \ No space left on device
 29 constant ESPIPE          \ Illegal seek
 30 constant EROFS           \ Read-only file system
 31 constant EMLINK          \ Too many links
 32 constant EPIPE           \ Broken pipe
 33 constant EDOM            \ Math argument out of domain of func
 34 constant ERANGE          \ Math result not representable
 35 constant EDEADLK         \ Resource deadlock would occur
 36 constant ENAMETOOLONG    \ File name too long
 37 constant ENOLCK          \ No record locks available
 38 constant ENOSYS          \ Invalid system call number
 39 constant ENOTEMPTY       \ Directory not empty
 40 constant ELOOP           \ Too many symbolic links encountered
 42 constant ENOMSG          \ No message of desired type
 43 constant EIDRM           \ Identifier removed
 44 constant ECHRNG          \ Channel number out of range
 45 constant EL2NSYNC        \ Level 2 not synchronized
 46 constant EL3HLT          \ Level 3 halted
 47 constant EL3RST          \ Level 3 reset
 48 constant ELNRNG          \ Link number out of range
 49 constant EUNATCH         \ Protocol driver not attached
 50 constant ENOCSI          \ No CSI structure available
 51 constant EL2HLT          \ Level 2 halted
 52 constant EBADE           \ Invalid exchange
 53 constant EBADR           \ Invalid request descriptor
 54 constant EXFULL          \ Exchange full
 55 constant ENOANO          \ No anode
 56 constant EBADRQC         \ Invalid request code
 57 constant EBADSLT         \ Invalid slot
 59 constant EBFONT          \ Bad font file format
 60 constant ENOSTR          \ Device not a stream
 61 constant ENODATA         \ No data available
 62 constant ETIME           \ Timer expired
 63 constant ENOSR           \ Out of streams resources
 64 constant ENONET          \ Machine is not on the network
 65 constant ENOPKG          \ Package not installed
 66 constant EREMOTE         \ Object is remote
 67 constant ENOLINK         \ Link has been severed
 68 constant EADV            \ Advertise error
 69 constant ESRMNT          \ Srmount error
 70 constant ECOMM           \ Communication error on send
 71 constant EPROTO          \ Protocol error
 72 constant EMULTIHOP       \ Multihop attempted
 73 constant EDOTDOT         \ RFS specific error
 74 constant EBADMSG         \ Not a data message
 75 constant EOVERFLOW       \ Value too large for defined data type
 76 constant ENOTUNIQ        \ Name not unique on network
 77 constant EBADFD          \ File descriptor in bad state
 78 constant EREMCHG         \ Remote address changed
 79 constant ELIBACC         \ Can not access a needed shared library
 80 constant ELIBBAD         \ Accessing a corrupted shared library
 81 constant ELIBSCN         \ .lib section in a.out corrupted
 82 constant ELIBMAX         \ Attempting to link in too many shared libraries
 83 constant ELIBEXEC        \ Cannot exec a shared library directly
 84 constant EILSEQ          \ Illegal byte sequence
 85 constant ERESTART        \ Interrupted system call should be restarted
 86 constant ESTRPIPE        \ Streams pipe error
 87 constant EUSERS          \ Too many users
 88 constant ENOTSOCK        \ Socket operation on non-socket
 89 constant EDESTADDRREQ    \ Destination address required
 90 constant EMSGSIZE        \ Message too long
 91 constant EPROTOTYPE      \ Protocol wrong type for socket
 92 constant ENOPROTOOPT     \ Protocol not available
 93 constant EPROTONOSUPPORT \ Protocol not supported
 94 constant ESOCKTNOSUPPORT \ Socket type not supported
 95 constant EOPNOTSUPP      \ Operation not supported on transport endpoint
 96 constant EPFNOSUPPORT    \ Protocol family not supported
 97 constant EAFNOSUPPORT    \ Address family not supported by protocol
 98 constant EADDRINUSE      \ Address already in use
 99 constant EADDRNOTAVAIL   \ Cannot assign requested address
100 constant ENETDOWN        \ Network is down
101 constant ENETUNREACH     \ Network is unreachable
102 constant ENETRESET       \ Network dropped connection because of reset
103 constant ECONNABORTED    \ Software caused connection abort
104 constant ECONNRESET      \ Connection reset by peer
105 constant ENOBUFS         \ No buffer space available
106 constant EISCONN         \ Transport endpoint is already connected
107 constant ENOTCONN        \ Transport endpoint is not connected
108 constant ESHUTDOWN       \ Cannot send after transport endpoint shutdown
109 constant ETOOMANYREFS    \ Too many references: cannot splice
110 constant ETIMEDOUT       \ Connection timed out
111 constant ECONNREFUSED    \ Connection refused
112 constant EHOSTDOWN       \ Host is down
113 constant EHOSTUNREACH    \ No route to host
114 constant EALREADY        \ Operation already in progress
115 constant EINPROGRESS     \ Operation now in progress
116 constant ESTALE          \ Stale file handle
117 constant EUCLEAN         \ Structure needs cleaning
118 constant ENOTNAM         \ Not a XENIX named type file
119 constant ENAVAIL         \ No XENIX semaphores available
120 constant EISNAM          \ Is a named type file
121 constant EREMOTEIO       \ Remote I/O error
122 constant EDQUOT          \ Quota exceeded
123 constant ENOMEDIUM       \ No medium found
124 constant EMEDIUMTYPE     \ Wrong medium type
125 constant ECANCELED       \ Operation Canceled
126 constant ENOKEY          \ Required key not available
127 constant EKEYEXPIRED     \ Key has expired
128 constant EKEYREVOKED     \ Key has been revoked
129 constant EKEYREJECTED    \ Key was rejected by service
130 constant EOWNERDEAD      \ Owner died
131 constant ENOTRECOVERABLE \ State not recoverable
132 constant ERFKILL         \ Operation not possible due to RF-kill
133 constant EHWPOISON       \ Memory page has hardware error

previous definitions

: ?err ( result -- result )
\ raises an exception if the result is an error
  dup 0 < over -1024 > and if \ error
    not 1+ abort" syscall" \ use positive errno in abort
  then
;

: ?-err ( result -- )
\ check for error
  ?err drop
;

: call7 syscall cr ;
: call6 0 swap call7 ;
: call5 0 swap call6 ;
: call4 0 swap call5 ;
: call3 0 swap call4 ;
: call2 0 swap call3 ;
: call1 0 swap call2 ;
: call0 0 swap call1 ;

voc: call
: exit ( code -- does-not-return )
  1 call1 ?-err ;

: read ( fd ptr len -- result )
  3 call3 ?err ;

: write ( fd ptr len -- result )
  4 call3 ?err ;

: close ( fd -- )
  6 call1 ?-err ;

: creat ( ptr len mode -- result )
  8 call3 ?err ;
: create creat inline ;

: dup ( fd -- fd2 )
  41 call1 ?err ;

: fcntl ( fd cmd arg -- result )
  55 call3 ?err ;

: pipe ( -- fd1 fd2 )
  0 0 sp@  42 call1  ?-err ;

previous definitions

voc: F_
0 constant DUPFD
1 constant GETFD
2 constant SETFD
3 constant GETFL
4 constant SETFL
5 constant GETLK
6 constant SETLK
7 constant SETLKW
8 constant SETOWN
9 constant GETOWN
10 constant SETSIG
11 constant GETSIG
12 constant GETLK64
13 constant SETLK64
14 constant SETLKW64
15 constant SETOWN_EX
16 constant GETOWN_EX
17 constant GETOWNER_UIDS

previous definitions

voc: O_
8 base !
00000003 constant ACCMODE
00000000 constant RDONLY
00000001 constant WRONLY
00000002 constant RDWR
00000100 constant CREAT
00000200 constant EXCL
00000400 constant NOCTTY
00001000 constant TRUNC
00002000 constant APPEND
00004000 constant NONBLOCK
00010000 constant DSYNC
00020000 constant FASYNC
00040000 constant DIRECT
00100000 constant LARGEFILE
00200000 constant DIRECTORY
00400000 constant NOFOLLOW
01000000 constant NOATIME
02000000 constant CLOEXEC

NONBLOCK constant NDELAY
decimal

previous definitions

: nonblock ( fd -- )
  dup F_ GETFL 0 call fcntl
  ( fd flags )
  O_ NONBLOCK or F_ SETFL swap call fcntl drop
;

: block ( fd -- )
  dup F_ GETFL 0 call fcntl
  ( fd flags )
  O_ NONBLOCK bic F_ SETFL swap call fcntl drop
;

class: timespec
__data
  var> int field: sec
  var> int field: nsec
__seal
: @ ( ts -- frac int )
  dup __ sec @ swap __ nsec @ 0 swap 0 1000000000 f/ drop swap ;
: ? ( ts -- )
  __ @ 6 f.n ;
: ! ( frac int ts -- )
  tuck __ sec !
  swap 0 0 1000000000 f* nip swap __ nsec !
  ;
: get ( clock# ts -- )
  263 call2 ?-err ;
: sleep ( ts -- )
  dup 162 call2 ?-err ;

;class

timespec class: monotonic
\ the monotonic clock: seconds since (re)boot
: get ( ts -- )
  1 __ get
;
;class

timespec class: realtime
\ realtime clock: seconds since 1970-01-01 00:00:00 UTC (excluding leap seconds)
: get ( ts -- )
  0 __ get
;
;class

class: epoll_event
__data
  var> int field: events
  cell+ \ arm nonsense?
  var> int field: u32
  cell+
__seal
;class

voc: epoll
: create ( -- result )
  1 250 call1 ;
: wait ( fd evt ts -- result )
  1 swap 0 call5 ;
: ctl ( fd op fd evt -- result )
  251 call4 ;

\ epoll_ctl_*
1 constant _ADD
2 constant _DEL
3 constant _MOD

\ Epoll event masks
1  0 lshift constant =IN
1  1 lshift constant =PRI
1  2 lshift constant =OUT
1  3 lshift constant =ERR
1  4 lshift constant =HUP
1  5 lshift constant =NVAL
1  6 lshift constant =RDNORM
1  7 lshift constant =RDBAND
1  8 lshift constant =WRNORM
1  9 lshift constant =WRBAND
1 10 lshift constant =MSG
1 13 lshift constant =RDHUP
1 28 lshift constant =EXCLUSIVE
1 29 lshift constant =WAKEUP
1 30 lshift constant =ONESHOT
1 31 lshift constant =ET

class: epcb
__data
  var> int field: fd
  epoll_event field: evt
  timespec field: timeout
#if-flag multi
  task %queue field: waiters
#endif
__seal

: setup ( addr -- )
#if-flag multi
  dup __ waiters >setup
#endif
  O_ CLOEXEC 357 call1 ?err
  swap __ fd !
;

#if-flag multi
: (tear) ( task -- )
  err EBADF swap  task %cls signal
;
#endif

: teardown ( addr -- )
  dup __ teardown
  dup __ fd @  call close
#if-flag multi
  __ waiters each: (tear)
#else
  drop
#endif
;

: (wait) ( fd mode epcb -- )
\ wait for this event
  >r
  =ONESHOT or  r@ __ evt events !
  ( fd |R: epcb )
#if-flag multi
  task this ..
#else
  dup
#endif
  r@ __ evt u32 !

  dup ( fd fd )
  r@ __ fd @ _MOD rot ( fd  epfd MOD fd )
  r@ __ evt ..
  251 call4 ( fd result |R: epcb )
  dup err ENOENT + if \ not that
    nip ?-err
  else
    drop
    r@ __ fd @ _ADD rot ( fd  epfd MOD fd )
    r@ __ evt ..
    251 call4 ?-err
  then
#if-flag multi
  r> __ waiters wait
#else
  rdrop
#endif
;

\ Unlike select/poll, EPOLL doesn't handle multiple requests on the
\ same file descriptor.
\ If you want to both read and write, clone the descriptor with DUP.
\ 
\ if we're not multitasking, these don't wait. Instead "poll" returns our
\ FD when it's read- or writeable.

: wait-read ( fd epcb -- flag )
\ wait for fd to be readable
  =IN swap __ (wait) ;

: wait-write ( fd epcb -- flag )
\ wait for fd to be writeable
  =OUT swap __ (wait) ;


: poll  ( f,|-1 epcb -- work? )
\ Wait until the timeout runs out or a registered epoll succeeds.
  >r
  0 r@ __ evt events !
  0 r@ __ evt u32 !
  dup 0 >= if r@ __ timeout ! 0 then
  ( flag |R: epcb )
  dup ( flag flag )
  r@ __ fd @  r@ evt ..  rot 1 swap ( flag  epfd evt 1 flag )
  if 0 else r@ timeout .. then
  0  \ no sigmask
  441 sys call5 \ epoll_pwait2
  dup err ENOSYS + if
    nip ( err )
  else
    \ our kernel / emulator doesn't do "epoll_pwait2". Sigh.
    drop ( flag )
    r@ __ fd @  r@ evt ..  rot 1 swap ( epfd evt 1 flag )
    if -1 else r@ __ timeout @ 1000, f* nip then
    252 sys call4 \ epoll_wait
  then
  ?err
  if
    r> __ evt u32 @
#if-flag multi
    task %cls continue  0
#endif
  else
    rdrop -1
  then
;


;class

forth only definitions

#endif
\ syscall

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
