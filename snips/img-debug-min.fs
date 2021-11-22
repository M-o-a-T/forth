#set-flag debug
#set-flag machid=123456789

#include sys/init.fs
#include lib/alloc.fs
#include lib/syscall.fs
#include debug/voc.fs
#include debug/alloc.fs
#include debug/class.fs
#include debug/crash.fs
#include debug/crash2.fs
#include debug/linked-list.fs
#include lib/disasm.fs
#include lib/dump.fs
#include lib/crc.fs
#include lib/save.fs

\file save" img/debug-base-min.img"
