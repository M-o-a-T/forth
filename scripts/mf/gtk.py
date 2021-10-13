import gi
gi.require_version('Vte', '2.91')
from gi.repository import Gtk, Vte, Gdk, Pango, GLib, Gio
#Gtk, Vte, and GLib are required.

import io
import os
#os.environ['HOME'] helps to keep from hard coding the home string.
#os is not required unless you want that functionality.

import trio

from .dummy import NoWindow, Evt, Data, SendFile, SendBuffer, StopSendFile

class Window(NoWindow):

    sendfile_dialog = None
    did_lf = True

    def __init__(self, title):
        super().__init__(title)

        self.history = []
        self.pos = 0
        self.last = ""
        self.sendfile_path = "."

        self.in_w,self.in_r = trio.open_memory_channel(10)

        self.widgets = Gtk.Builder()
        self.widgets.add_from_file("scripts/mf/gtk.glade")

        self.build_window(title)
        self.clip = Gtk.Clipboard.get(Gdk.SELECTION_CLIPBOARD)

        d = {k:getattr(self,k) for k in dir(self) if k[0] != "_"}

        self.widgets.connect_signals(d)

    def __getitem__(self,name):
        return self.widgets.get_object(name)

    def build_window(self, title):
        """Build the GUI window"""

        # the window itself
        self.window = self['main']

        # the menu button
        pmenu = self['menu']
        pmenu.show_all()

        # a button to get the menu to pop up

        #box = Gtk.Box(orientation=Gtk.Orientation.HORIZONTAL)
        #box.add(mb)

        # our header bar, mainly for the menu button
        mbut = self["menubutton"]
        if mbut is None:
            mbut = Gtk.MenuButton()

            hb = Gtk.HeaderBar()
            hb.set_show_close_button(True)
            hb.props.title = title
            self.window.set_titlebar(hb)  # attach it

            hb.pack_start(mbut)
            hb.show_all()

        icon = Gio.ThemedIcon(name="open-menu-symbolic")
        image = Gtk.Image.new_from_gicon(icon, Gtk.IconSize.BUTTON)
        mbut.add(image)
        mbut.set_popup(pmenu)

        # Terminal. It has an inobtrusive cursor and cannot focus.
        self.terminal = Vte.Terminal()
        self.terminal.set_cursor_shape(Vte.CursorShape.UNDERLINE)
        self.terminal.set_color_cursor_foreground(Gdk.RGBA(0,0,0,0))
        self.terminal.set_can_focus(False)
        term_scroller = self['term_scroller']
        term_scroller.set_hexpand(True)
        term_scroller.set_vexpand(True)
        term_scroller.add(self.terminal)

        # Text entry
        self.entry = self['text_out']
        # self.entry.connect("activate", self.send_text)
        # self.entry.connect("key_press_event", self.key_pressed)
        font = Pango.font_description_from_string("Monospace 11")
        self.entry.modify_font(font)

        # accel. Doesn't work via Glade.
        acgroup = Gtk.AccelGroup()
        self.window.add_accel_group(acgroup)

        self["menu_send"].add_accelerator("activate", acgroup, ord("O"), Gdk.ModifierType.CONTROL_MASK, Gtk.AccelFlags.VISIBLE)
        self["menu_quit"].add_accelerator("activate", acgroup, ord("Q"), Gdk.ModifierType.CONTROL_MASK, Gtk.AccelFlags.VISIBLE)

        # finish
        self.window.show_all()
        self.stopped_sending()
        self.entry.grab_focus()

    def do_quit(self, *x):
        Gtk.main_quit()

    def started_sending(self):
        self["btn_stop"].show()
        self["btn_send"].hide()

    def stopped_sending(self):
        self["btn_stop"].hide()
        self["btn_send"].show()

    def do_btn_stop(self, *x):
        """stop sending a file"""
        self.in_w.send_nowait(StopSendFile())

    def do_menu_send(self, *x):
        """send a file"""
        if self.sendfile_dialog is None:
            self.sendfile_dialog = d = self["sendfile_chooser"]
            d.set_default_response(Gtk.ResponseType.OK)
        else:
            d = self.sendfile_dialog
        d.set_current_folder(self.sendfile_path)
        d.present()
        d.show()
        return

    def do_sendfile_this(self, widget):
        filename = widget.get_filename()
        d = self.sendfile_dialog

        self.sendfile_path = d.get_current_folder()
        d.hide()
        self.in_w.send_nowait(SendFile(filename))
        return True

    def do_sendfile_close(self, *x):
        self.sendfile_dialog.hide()
        return True

    def sendfile_filter_esc(self, widget, evt):
        d = self.sendfile_dialog
        while Gtk.events_pending():
            Gtk.main_iteration()
        keyname = Gdk.keyval_name(evt.keyval)
        if keyname != "Escape":
            return False
        d.hide()
        return True

    def sendfile_close(self):
        d.hide()

    def do_xx(self, *x):
        self.sendfile_dialog.hide()
        return True

    def sendfile_response(self, widget, response):
        d = self.sendfile_dialog
        if response == Gtk.ResponseType.OK:
            filename = d.get_filename()
            self.in_w.send_nowait(SendFile(filename))
            d.hide()
            self.sendfile_path = d.get_current_folder()
            return True
        elif response == Gtk.ResponseType.CANCEL:
            d.hide()
            return True
        else:
            print("UNKNOWN RESP",response)
        return


    def do_btn_send(self, widget):
        """send this text"""
        txt = self.entry.get_text()
        self.in_w.send_nowait(txt)
        self.entry.set_text("")
        if txt != "":
            try:
                self.pos = self.history.index(txt)
            except ValueError:
                self.history.append(txt)
                self.pos = len(self.history)
        return True

    def key_pressed(self, widget, data):
        """filter keypresses"""
        while Gtk.events_pending():
            Gtk.main_iteration()
        keyname = Gdk.keyval_name(data.keyval)
        if keyname == "Enter":
            self.do_btn_send(widget)
        elif keyname == "Escape":
            self.in_w.send_nowait(StopSendFile())
            return True
        elif keyname == "Tab":
            self.entry.set_position(-1)
            return True
        elif keyname not in ("Up", "Down"):
            return False
        elif not self.history:
            return
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
        if self.pos < len(self.history):
            self.entry.set_text(self.history[self.pos])
            self.entry.select_region(0,-1)
        return True

    def send(self, data, lf=False):
        """Display this."""
        if data == "":
            return
        if lf and data[0] != "\n" and not self.did_lf:
            self.terminal.feed(b"\r\n")
        self.did_lf = data[-1] == "\n"
        self.terminal.feed(data.replace("\n","\r\n").encode("utf-8"))

    def __aiter__(self):
        return self.in_r.__aiter__()

    def do_paste(self, widget, text):
        if "\n" in text:
            self.entry.stop_emission("paste-clipboard")
            self.send_paste(text)
            return True

    def do_insert(self, widget, text, *x):
        if "\n" in text:
            self.entry.stop_emission("insert-text")
            self.send_paste(text)
            return True

    def send_paste(self, text):
        f = io.StringIO()
        f.write(text+"\n")
        f.seek(0)
        self.in_w.send_nowait(SendBuffer(f))


if __name__ == "__main__":
    win = TheWindow()
    Gtk.main()
