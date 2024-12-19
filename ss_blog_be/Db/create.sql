--
-- File generated with SQLiteStudio v3.4.4 on Sat Dec 7 19:31:06 2024
--
-- Text encoding used: System
--
PRAGMA foreign_keys = off;
BEGIN TRANSACTION;

-- Table: content
CREATE TABLE IF NOT EXISTS content (objId TEXT, postId INTEGER REFERENCES post (ROWID), type NUMERIC NOT NULL);

-- Table: post
CREATE TABLE IF NOT EXISTS post (title TEXT NOT NULL, content TEXT, description TEXT, createdAt INTEGER, typeId INTEGER REFERENCES postType (ROWID), ROWID INTEGER PRIMARY KEY AUTOINCREMENT, isPublished INTEGER, tags TEXT);

-- Table: postFTS
CREATE VIRTUAL TABLE IF NOT EXISTS postFTS USING fts5(tags, content=post, content_rowid=ROWID);

-- Table: postFTS_v
CREATE VIRTUAL TABLE IF NOT EXISTS postFTS_v USING fts5vocab(postFTS, col);
-- Table: postType
CREATE TABLE IF NOT EXISTS postType (name TEXT NOT NULL UNIQUE, ROWID INTEGER PRIMARY KEY);
INSERT INTO postType (name, ROWID) VALUES ('article', 1);
INSERT INTO postType (name, ROWID) VALUES ('thought', 2);
INSERT INTO postType (name, ROWID) VALUES ('About', 3);
INSERT INTO postType (name, ROWID) VALUES ('Hero', 4);

COMMIT TRANSACTION;
PRAGMA foreign_keys = on;
