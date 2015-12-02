# Light Rail

The code here implements a persistent, append-only data structure.  It is similar to write ahead log (WAL)  
used in databases to keep the transaction log.

## How does it work?

Why would I use it  
Guarantees  
Moving parts  
Storage layout  
Performance

## Usage

api surface  
- append     
- iterate forward/backward  
- top/head

## Contributing

1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin my-new-feature`
5. Submit a pull request :D

## History

The effort invested here is a part of a long learning journey of building reliable distributed systems. In special  
crash-recovery process abstraction. 

## Credits

- lmdb
- eventstore
- kafka
- rocksdb    

## License

https://opensource.org/licenses/BSD-3-Clause