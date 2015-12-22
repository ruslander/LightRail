# Stable storage

The code here implements a persistent, append-only data structure.  It is similar to write ahead log (WAL)  
used in databases to keep the transaction log.

## How does it work?

Why would I use it  
Guarantees  
Moving parts  
Performance

## Usage

api surface  
- append     
- iterate forward/backward  
- top/head

##Storage layout  

```
Log format

	tail                                                     head 
	+---------------------------------------------+---- ... ----+
	|   f000000000000.sf   |   f000004194304.sf   |
	+---------------------------------------------+---- ... ----+
	<------ 4mb block ---->|<--- 4mb block ------->

	head = the active file which accepts append operations
	tail = will normaly be readonly files
	f000004194304.sf = represents next the 4mb of logs
   
File format

	+-----+-------------+--+----+----------+------+-- ... ----+
	| r0  |        r1   |P | r2 |    r3    |  r4  |           |
	+-----+-------------+--+----+----------+------+-- ... ----+
	<--- 4k block   ------>|<--   4k block ------>|

	rn = variable size records
	P = Padding

Block format

	+-----+-------+-----+----------+------+-- ... ----+----+----+
	| r0  |  r1   |  r2 |    r3    |  r4  |        s2 | s1 | s0 |
	+-----+-------+-----+----------+------+-- ... ----+----+----+
	|<--- 4k block   ------------------------------------------>|

	rn = records
	sn = record sizes

Record format

	+----------+------------+------------+--- ... ---+
	| Pos (8B) | Hash (16B) |  Payload   |
	+----------+------------+------------+--- ... ---+

	Pos = global record position in the stream
	Hash = 16B hash computed over the payload using MD5
	Payload = Byte stream as long as specified by the header size
```

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
- leveldb https://leveldb.googlecode.com/svn/trunk/doc/log_format.txt
- eventstore
- kafka
- rocksdb    

## License

https://opensource.org/licenses/BSD-3-Clause
