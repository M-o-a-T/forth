all: test

test: test/gen/bugtest.fs
	scripts/test

files: test/gen/bugtest.fs

test/gen/bugtest.fs: scripts/mapgen test/bugtest.svd
	scripts/mapgen test/bugtest.svd BFT > test/gen/bugtest.fs

DEV ?= /dev/ttyUSB0
rtest:
	scripts/test_real ${DEV}

.PHONY: files test all
