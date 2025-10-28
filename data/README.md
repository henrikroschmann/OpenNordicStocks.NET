# Nordic Stock Data

This directory contains normalized JSON snapshots of Nordic stock market data.

## Files

- `latest.json` - Most recent stock data snapshot
- `YYYY-MM-DD.json` - Historical snapshots for specific dates

## Data Structure

Each JSON file contains:
- `date` - Date of the snapshot
- `stocks` - Array of stock data objects
- `metadata` - Snapshot metadata including version, generation time, total count, and markets

## Markets Covered

- OMX Stockholm (Sweden)
- OMX Helsinki (Finland)
- OMX Copenhagen (Denmark)

## Update Schedule

Data is automatically updated daily at 6:00 PM UTC via GitHub Actions.

