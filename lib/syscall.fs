forth only definitions

#if undefined syscall
#end
#endif

#if defined sys
#end
#endif

#require class: lib/class.fs

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
;voc

voc: ippproto
0 constant ip
1 constant icmp
6 constant tcp
17 constant udp
41 constant ipv6 \ header
47 constant gre
98 constant encap
143 constant ethernet
255 constant raw
;voc

voc: pf
0 constant unspec
1 constant unix
1 constant local
1 constant file
2 constant inet
4 constant ipx
10 constant inet6
16 constant netlink
16 constant route
17 constant packet
29 constant can
31 constant bluetooth
;voc

voc: sig
1 constant ign
0 constant dfl
-1 constant err

64 constant _nsig
1 constant HUP
2 constant INT
3 constant QUIT
4 constant ILL
5 constant TRAP
6 constant ABRT
6 constant IOT
7 constant BUS
8 constant FPE
9 constant KILL
10 constant USR1
11 constant SEGV
12 constant USR2
13 constant PIPE
14 constant ALRM
15 constant TERM
16 constant STKFLT
17 constant CHLD
18 constant CONT
19 constant STOP
20 constant TSTP
21 constant TTIN
22 constant TTOU
23 constant URG
24 constant XCPU
25 constant XFSZ
26 constant VTALRM
27 constant PROF
28 constant WINCH
29 constant IO
IO constant POLL
29 constant LOST
30 constant PWR
31 constant SYS
31 constant UNUSED
32 constant RTMIN
_NSIG constant RTMAX

voc: ~bus
1 constant ADRALN
2 constant ADRERR
3 constant OBJERR
4 constant MCEERR_AR
5 constant MCEERR_AO
;voc

voc: ~ill
1 constant OPC
2 constant OPN
3 constant ADR
4 constant TRP
5 constant PRVOPC
6 constant PRVREG
7 constant COPROC
8 constant BADSTK
9 constant BADIADDR
10 constant BREAK
11 constant BNDMOD
;voc

voc: ~segv
1 constant MAPERR
2 constant ACCERR
3 constant BNDERR
4 constant PKUERR
5 constant ACCADI
6 constant ADIDERR
7 constant ADIPERR
8 constant MTEAERR
9 constant MTESERR
;voc

voc: ~sa
$00000001 constant NOCLDSTOP
$00000002 constant NOCLDWAIT
$00000004 constant SIGINFO
$08000000 constant ONSTACK
$10000000 constant RESTART
$40000000 constant NODEFER
$80000000 constant RESETHAND
NODEFER constant NOMASK
RESETHAND constant ONESHOT
;voc

class: info
__data
  var> int field: signo
  var> int field: errno
  var> int field: code
  var> int field: addr \ bus/segv: addr; 
  var> int field: uid \ kill
__seal

var> int item
: pid __ addr .. inline ;
;class

class: action
__data
  var> int field: handler
  var> int field: flags
  var> int field: mask
__seal
;class

class: altstack
__data
  var> int field: sp
  var> int field: flags
  var> int field: size
__seal
;class

;voc \ sig

: af postpone pf immediate ;

voc: sock
1 constant stream
2 constant dgram
3 constant raw
5 constant seqpacket
10 constant packet
$800 constant nonblock
$80000 constant cloexec
;voc


: ?err ( result -- result )
\ raises an exception if the result is an error
  dup 0 < over -1024 > and if \ error
    negate abort" syscall"
  then
;

: ?-err ( result -- )
\ check for error
  ?err drop
;

: ramhere
  compiletoram? 0= abort" no flash"
  here
;

: call7 syscall ;
: call6 0 swap call7 ;
: call5 0 swap call6 ;
: call4 0 swap call5 ;
: call3 0 swap call4 ;
: call2 0 swap call3 ;
: call1 0 swap call2 ;
: call0 0 swap call1 ;

voc: call
: >fn0 ( adr len -- )
\ zero terminate a filename. Stored at @here.
  tuck ramhere swap move ( len )
  ramhere + 0 swap c!
;

: exit ( code -- does-not-return )
  1 call1 ?-err ;

: read ( fd ptr len -- result )
  3 call3 ?err ;

: write ( fd ptr len -- result )
  4 call3 ?err ;

: open ( ptr len flags mode -- result )
  2>r >fn0 2r>
  ramhere -rot 5 call3 ?err ;

: close ( fd -- )
  6 call1 ?-err ;

: creat ( ptr len mode -- result )
  -rot >fn0
  ramhere swap 8 call2 ?err ;
: create creat inline ;

: dupfd ( fd -- fd2 )
\ don't call it "dup"!
  41 call1 ?err ;

: fcntl ( fd cmd arg -- result )
  55 call3 ?err ;

: pipe ( -- fd1 fd2 )
  0 0 sp@  42 call1  ?-err ;

: socket ( dom typ prot -- fd )
  281 call3 ?err ;

: bind ( fd addr len -- )
  282 call3 ?-err ;

: connect ( fd adr len -- )
  283 call3 ?-err ;

: getpid ( -- pid )
  20 call0 ?err ;

: kill ( pid sig -- )
  37 call2 ?-err ;

: signal ( xt signum -- )
\ XT must be "sigenter foo sigexit"
\ we ignore the old state
  swap 
  ramhere sig action >setup
  dup 1 > if 1 or then \ thumb bit
  ramhere sig action handler !

  ramhere 0 ( sig new old )
  67 call3 ?-err ;

;voc

voc: F_
decimal
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

;voc

8 base !

voc: S_
00444 constant IRUSR
00222 constant IWUSR
00111 constant IXUSR
;voc

voc: O_
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

;voc

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
  1 swap __ get
;
;class

timespec class: realtime
\ realtime clock: seconds since 1970-01-01 00:00:00 UTC (excluding leap seconds)
: get ( ts -- )
  0 swap __ get
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
  \ timespec field: timeout \ ignored, we use msec
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
\ If you want to poll on both read and write, clone the descriptor with DUP.
\ 
\ if we're not built for multitasking, these functions don't wait.
\ Instead, "poll" returns the FD when it's read- or writeable.

: wait-read ( fd epcb -- flag )
\ wait for fd to be readable
  =IN swap __ (wait) ;

: wait-write ( fd epcb -- flag )
\ wait for fd to be writeable
  =OUT swap __ (wait) ;

: poll  ( µs|0 epcb -- work? )
\ Wait until the timeout runs out or a registered epoll succeeds.
\ Return -1 if no work. Otherwise on multitask return 0, singletask
\ returns the file descriptor that is ready.
  >r
  0 r@ __ evt events !
  0 r@ __ evt u32 !
  ?dup if 1000 / else -1 then \ -1 for "forever"
  ( timeout |R: epcb )
  begin
    r@ __ fd @  r@ evt ..  rot 1 swap ( epfd evt 1 timeout )
    252 sys call4 \ epoll_wait
    dup  err EINTR +
  until  \ loop if EINTR
  ?err
  ( 0|1 )
  if
    r>
    __ evt u32 @
#if-flag multi
    \ if we're multitasking, the epoll evt contains the task that should wake up.
    task %cls continue  false
#endif
    \ Otherwise it's the file descriptor, which we return directly.
  else
    rdrop true
    \ Nothing to do, timed out.
  then
;

;class


forth definitions

#if-flag multi
epcb object: poll
#endif

forth only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
