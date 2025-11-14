# Coturn TURN Server Setup

## Installation

### Ubuntu/Debian
```bash
sudo apt-get update
sudo apt-get install coturn
```

### CentOS/RHEL
```bash
sudo yum install coturn
```

### Docker
```bash
docker run -d --network=host \
  -v $(pwd)/config/turnserver.conf:/etc/coturn/turnserver.conf \
  coturn/coturn
```

## Configuration

1. **Copy configuration file**
```bash
sudo cp turnserver.conf /etc/coturn/turnserver.conf
```

2. **Update external IP**
Edit `/etc/coturn/turnserver.conf` and replace `YOUR_PUBLIC_IP` with your server's public IP:
```conf
external-ip=YOUR_PUBLIC_IP
```

3. **Enable Coturn service**
```bash
sudo systemctl enable coturn
sudo systemctl start coturn
```

4. **Check status**
```bash
sudo systemctl status coturn
```

## Firewall Configuration

### UFW (Ubuntu)
```bash
sudo ufw allow 3478/tcp
sudo ufw allow 3478/udp
sudo ufw allow 5349/tcp
sudo ufw allow 5349/udp
sudo ufw allow 49152:65535/tcp
sudo ufw allow 49152:65535/udp
```

### Firewalld (CentOS)
```bash
sudo firewall-cmd --permanent --add-port=3478/tcp
sudo firewall-cmd --permanent --add-port=3478/udp
sudo firewall-cmd --permanent --add-port=5349/tcp
sudo firewall-cmd --permanent --add-port=5349/udp
sudo firewall-cmd --permanent --add-port=49152-65535/tcp
sudo firewall-cmd --permanent --add-port=49152-65535/udp
sudo firewall-cmd --reload
```

## Testing

Test STUN server:
```bash
stunclient YOUR_PUBLIC_IP 3478
```

Test TURN server:
```bash
turnutils_uclient -u tunrtc -w tunrtc123 YOUR_PUBLIC_IP
```

## Credentials

Default credentials configured:
- **Username**: tunrtc
- **Password**: tunrtc123

**Important**: Change these in production!

## Integration with TunRTC

Update `appsettings.json` in the Server project:

```json
{
  "IceServers": {
    "TurnServer": {
      "Enabled": true,
      "Urls": [
        "turn:YOUR_PUBLIC_IP:3478"
      ],
      "Username": "tunrtc",
      "Credential": "tunrtc123"
    }
  }
}
```

## Monitoring

View logs:
```bash
sudo tail -f /var/log/coturn/turnserver.log
```

## Troubleshooting

### Port already in use
```bash
sudo lsof -i :3478
sudo netstat -tulpn | grep 3478
```

### Check connectivity
```bash
telnet YOUR_PUBLIC_IP 3478
```

### Restart service
```bash
sudo systemctl restart coturn
```

## Security Recommendations

1. Use strong credentials
2. Enable TLS/DTLS in production
3. Restrict IP ranges if possible
4. Use quota limits to prevent abuse
5. Monitor logs regularly
6. Keep Coturn updated

## Performance Tuning

For high-traffic scenarios, adjust in `turnserver.conf`:
```conf
max-bps=5000000
total-quota=500
user-quota=50
```

## Resources

- [Coturn GitHub](https://github.com/coturn/coturn)
- [RFC 5766 - TURN](https://tools.ietf.org/html/rfc5766)
- [RFC 5389 - STUN](https://tools.ietf.org/html/rfc5389)
