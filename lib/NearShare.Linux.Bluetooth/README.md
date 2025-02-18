# NearShare.Linux.Bluetooth
Bluetooth pal implementation for libCdp.

It implements sdp lookup via `sdp_connect` to discover the `RFCOMM` channel of the well-known cdp uuid.   
Then a `RFCOMM` connection is established via `socket`.

We might need to enable ertm for the `rfcomm` kernel module??
```bash
echo 1 | sudo tee /sys/module/rfcomm/parameters/l2cap_ertm
```

## Help

- https://people.csail.mit.edu/rudolph/Teaching/Articles/BTBook.pdf

### Sdp
- https://github.com/bluez/bluez/blob/master/lib/sdp_lib.h
- https://github.com/blueman-project/blueman/blob/64f9cfb5201232b58d109387285d59c27ee434f8/module/libblueman.c

### Rfcomm
- https://github.com/bluez/bluez/wiki/RFCOMM(7)
