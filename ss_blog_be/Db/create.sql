--
-- File generated with SQLiteStudio v3.4.4 on Sun Dec 28 23:48:03 2025
--
-- Text encoding used: System
--
PRAGMA foreign_keys = off;
BEGIN TRANSACTION;

-- Table: content
CREATE TABLE IF NOT EXISTS content (objId TEXT, postId INTEGER REFERENCES post (ROWID), type NUMERIC NOT NULL);

-- Table: post
CREATE TABLE IF NOT EXISTS post (title TEXT NOT NULL, content TEXT, description TEXT, createdAt INTEGER, typeId INTEGER REFERENCES postType (ROWID), ROWID INTEGER PRIMARY KEY AUTOINCREMENT, isPublished INTEGER, tags TEXT, tagsCodeSnippets TEXT);

-- Table: postFTS
CREATE VIRTUAL TABLE IF NOT EXISTS postFTS USING fts5(tags, tagsCodeSnippets , content=post, content_rowid=ROWID);

-- Table: postFTS_config
CREATE TABLE IF NOT EXISTS 'postFTS_config'(k PRIMARY KEY, v) WITHOUT ROWID;
INSERT INTO postFTS_config (k, v) VALUES ('version', 4);

-- Table: postFTS_data
CREATE TABLE IF NOT EXISTS 'postFTS_data'(id INTEGER PRIMARY KEY, block BLOB);

-- Table: postFTS_docsize
CREATE TABLE IF NOT EXISTS 'postFTS_docsize'(id INTEGER PRIMARY KEY, sz BLOB);

-- Table: postFTS_idx
CREATE TABLE IF NOT EXISTS 'postFTS_idx'(segid, term, pgno, PRIMARY KEY(segid, term)) WITHOUT ROWID;

-- Table: postFTS_v
CREATE VIRTUAL TABLE IF NOT EXISTS postFTS_v USING fts5vocab(postFTS, col);


-- Table: postType
CREATE TABLE IF NOT EXISTS postType (name TEXT NOT NULL UNIQUE, ROWID INTEGER PRIMARY KEY);
INSERT INTO postType (name, ROWID) VALUES ('article', 1);
INSERT INTO postType (name, ROWID) VALUES ('thought', 2);
INSERT INTO postType (name, ROWID) VALUES ('About', 3);
INSERT INTO postType (name, ROWID) VALUES ('Hero', 4);
INSERT INTO postType (name, ROWID) VALUES ('Code Snippet', 5);

COMMIT TRANSACTION;
PRAGMA foreign_keys = on;
