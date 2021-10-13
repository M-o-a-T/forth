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

    def set_error(self, e):
        print(f"--- error: {e} ---", file=sys.stderr)

    def set_info(self, e):
        print(f"--- info: {e} ---", file=sys.stderr)

    def set_fileinfo(self, name,num):
        pass

    def close(self):
        pass

class Evt:                        
    """generic event to send to the app"""
    pass
        
class StopSendFile(Evt):
    def __repr__(self):
        return f"<{self.__class__.__name__}>"

class Data(Evt):
    def __init__(self, data):
        self.data = data
    def __repr__(self):
        return f"<{self.__class__.__name__} data={self.data!r}>"
    
class SendFile(Data):
    pass

class SendBuffer(Data):
    pass

