all: test

test: test/gen/bugtest.fs
	scripts/test

files: test/gen/bugtest.fs

test/gen/bugtest.fs: scripts/mapgen test/bugtest.svd
	scripts/mapgen test/bugtest.svd BFT > test/gen/bugtest.fs

.PHONY: files test all
