all: images prep test

ARCH ?= armcm3
VENDOR ?= STMicro
MCU ?= STM32F103xx
CLK ?= 72000000
MECRISP ?= ../mecrisp-stellaris/linux-ra/mecrisp-stellaris-linux

prep:: .done

prep:: img

img:
	mkdir img

images: img
	scripts/mf-term -b -x snips/img-debug-base.fs -c $(MECRISP)
	scripts/mf-term -b -x snips/img-debug-min.fs -c $(MECRISP)
	scripts/mf-term -b -x snips/img-debug.fs -c $(MECRISP)
	scripts/mf-term -b -x snips/img.fs -c $(MECRISP)

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

test: prep test/gen/bugtest.fs
	scripts/test
	scripts/testdocs

files: prep test/gen/bugtest.fs

test/gen/bugtest.fs: scripts/mapgen test/bugtest.svd
	scripts/mapgen test/bugtest.svd BFT > test/gen/bugtest.fs

DEV ?= /dev/ttyUSB0
rtest: prep
	scripts/test_real ${DEV} ${ARCH} ${VENDOR} ${MCU} ${CLK}

.PHONY: files test all prep
