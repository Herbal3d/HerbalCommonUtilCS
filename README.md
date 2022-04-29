# HerbalCommonUtilCS

Utility routines for the CS Herbal code:

-- *BHasher*: Wrapper class for creating hashes of data. Wraps hash codes as Long, ULong, Mdjb2 (from OpenSimulator), MD5, SHA256, and SHA512 with a common API so the caller doesn't know the underlying implementation.
-- *BTimeSpan*: a class function for timing a code section.
-- *Logger*: a common logger wrapper. Implementations for console, NLog, and Log4Net.
-- *IParameters*: a common structure for keeping key/value parameters
-- *ParamBlock*: implmentation of key/value storage that has an IParameters interface
-- *ServiceParameters*: another way of key/value parameter storage with an IParameters interface
-- *Util*: useful functions: Clamp, RandomString
