#
# A dummy UI that doesn't do anything.

import trio
import sys

async def _never_iter():
    if False:
        yield None
    while True:
        trio.sleep(99999)

class NoWindow:
    has_nl = True

    def __init__(self, title):
        pass

    def __aiter__(self):
        return _never_iter().__aiter__()

    async def send(self, data):
        pass

    async def set_error(self, e):
        print(f"--- error: {e} ---", file=sys.stderr)

    async def set_info(self, e):
        print(f"--- info: {e} ---", file=sys.stderr)

    async def set_fileinfo(self, name,num):
        pass

    def close(self):
        pass


