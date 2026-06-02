#!/bin/sh

# Set retention period (in days)
RETENTION_DAYS=7

# Clean up MySQL binary logs
mysql -h mysql -u root -p'aA@123456' -e "PURGE BINARY LOGS BEFORE DATE_SUB(NOW(), INTERVAL $RETENTION_DAYS DAY);"

# Clean up old backup files if they exist
find /var/lib/mysql -name "*.sql.gz" -type f -mtime +$RETENTION_DAYS -delete

# Clean up old log files
find /var/lib/mysql -name "*.log" -type f -mtime +$RETENTION_DAYS -delete
find /var/lib/mysql -name "*.err" -type f -mtime +$RETENTION_DAYS -delete

# Clean up temporary files
find /var/lib/mysql -name "*.tmp" -type f -mtime +1 -delete

# Log the cleanup
echo "[$(date)] Cleanup completed - removed files older than $RETENTION_DAYS days" 