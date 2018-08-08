# Torrentinator

Runs fine on Raspberry Pi. Requires a tor service running. Get one from: sudo apt-get tor
Configure /etc/tor/torrc:
1. Uncomment the ControlPort (should be 9051)
2. Set the hashed password to: 5FE757BCE9DF3D36601E7AEDAE86C10D7EA95B9707D0A9623B4F246A0C
3. restart with sudo service tor restart
  
Obviously, needs nginx to run. 
