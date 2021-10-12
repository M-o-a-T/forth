import gi
gi.require_version('Vte', '2.91')
from gi.repository import Gtk, Vte, Gdk, Pango, GLib
#Gtk, Vte, and GLib are required.


import os
#os.environ['HOME'] helps to keep from hard coding the home string.
#os is not required unless you want that functionality.

import trio

from .dummy import NoWindow

class Evt:
    """generic event to send to the app"""
    pass

class Data(Evt):
    def __init__(self):
        self.data = data

class Window(NoWindow):

    def __init__(self, title):
        super().__init__(title)
        self.window = Gtk.Window()
        self.window.set_title(title)

        self.history = []
        self.pos = 0
        self.last = ""

        self.in_w,self.in_r = trio.open_memory_channel(10)

        self.window.set_default_size(600, 300)
        self.terminal = Vte.Terminal()
        self.terminal.set_cursor_shape(Vte.CursorShape.UNDERLINE)

        font = Pango.font_description_from_string("Monospace 11")

        self.terminal.set_can_focus(False)
        scroller = Gtk.ScrolledWindow()
        scroller.set_hexpand(True)
        scroller.set_vexpand(True)
        scroller.add(self.terminal)

        self.entry = Gtk.Entry()
        self.entry.connect("activate", self.send_text)
        self.entry.connect("key_press_event", self.key_pressed)
        self.entry.modify_font(font)

        self.send_button = Gtk.Button("Send")
        self.send_button.connect("clicked", self.send_text)
        self.upload = Gtk.Button("Upload")

        hb_bot = Gtk.Box(orientation=Gtk.Orientation.HORIZONTAL)
        hb_bot.pack_start(self.entry, True, True, 0)
        hb_bot.pack_start(self.send_button, False, True, 0)
        hb_bot.pack_start(self.upload, False, True, 0)

        main = Gtk.Box(orientation=Gtk.Orientation.VERTICAL)
        main.pack_start(scroller, True, True, 0)
        main.pack_start(hb_bot, False, True, 0)
        self.window.add(main)
        
        self.window.set_border_width(2)
        self.window.connect("delete-event", Gtk.main_quit)

        self.entry.grab_focus()

        self.window.show_all()

    def send_text(self, widget):
        txt = self.entry.get_text()
        self.in_w.send_nowait(txt)
        self.entry.set_text("")
        if txt != "":
            try:
                self.pos = self.history.index(txt)
            except ValueError:
                self.history.append(txt)
                self.pos = len(self.history)
                print("pos:",self.pos)
        return True

    def key_pressed(self, widget, data):
        while Gtk.events_pending():
            Gtk.main_iteration()
        keyname = Gdk.keyval_name(data.keyval)
        if keyname == "Escape":
            self.destroy()
        elif keyname == "Tab":
            self.entry.set_position(-1)
            return True
        elif keyname not in ("Up", "Down"):
            return
        elif not self.history:
            return True
        self.entry.grab_focus()
        if len(self.history) == 0:
            return True
        if keyname == "Down":
            if self.pos == len(self.history):
                self.last = self.entry.get_text()
                self.pos = 0
            else:
                self.pos += 1
                if self.pos == len(self.history):
                    self.entry.set_text(self.last)
                    self.entry.set_position(-1)
        else:
            if self.pos == len(self.history):
                self.last = self.entry.get_text()
                self.entry.set_position(-1)
            if self.pos:
                self.pos -= 1
            else:
                self.entry.set_text(self.last)
                self.pos = len(self.history)
        print("pos",self.pos)
        if self.pos < len(self.history):
            self.entry.set_text(self.history[self.pos])
            self.entry.select_region(0,-1)
        return True

    async def send(self, data):
        """Display this."""
        self.terminal.feed(data.encode("utf-8"))

    def __aiter__(self):
        return self.in_r.__aiter__()


if __name__ == "__main__":
    win = TheWindow()
    win.show_all()
    Gtk.main()
