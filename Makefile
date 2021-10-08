all: prep regs test

VENDOR ?= STMicro
DEVICE ?= STM32F103xx

prep:: .done

.done:
	mkdir -p .done

prep:: .done/gen_core
.done/gen_core:
	scripts/gen_all ./svd/core/Device/ARM/SVD/ ./svd/fs/core/
	touch .done/gen_core

prep:: .done/gen_soc_${VENDOR}
.done/gen_soc_${VENDOR}:
	scripts/gen_all ./svd/soc/data/${VENDOR}/ ./svd/fs/soc/${VENDOR}/
	touch .done/gen_soc_${VENDOR}

regs: regs_core regs_soc

regs_core:
	mkdir -p 
	scripts/mapgen_all
	touch regs_core

test: test/gen/bugtest.fs
	scripts/test

files: test/gen/bugtest.fs

test/gen/bugtest.fs: scripts/mapgen test/bugtest.svd
	scripts/mapgen test/bugtest.svd BFT > test/gen/bugtest.fs

DEV ?= /dev/ttyUSB0
rtest:
	scripts/test_real ${DEV}

.PHONY: files test all regs prep
