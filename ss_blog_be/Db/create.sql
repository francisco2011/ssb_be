--
-- File generated with SQLiteStudio v3.4.8 on Tue Jan 20 19:38:19 2026
--
-- Text encoding used: System
--
PRAGMA foreign_keys = off;
BEGIN TRANSACTION;

-- Table: content
CREATE TABLE IF NOT EXISTS content (objId TEXT, postId INTEGER REFERENCES post (ROWID), type NUMERIC NOT NULL);

-- Table: post
CREATE TABLE IF NOT EXISTS post (title TEXT NOT NULL, 
								content TEXT, description TEXT, 
								createdAt INTEGER, 
								typeId INTEGER REFERENCES postType (ROWID), 
								ROWID INTEGER PRIMARY KEY AUTOINCREMENT, 
								isPublished INTEGER, 
								name TEXT);

CREATE TABLE IF NOT EXISTS tags(content TEXT, previousContent TEXT, ROWID INTEGER PRIMARY KEY AUTOINCREMENT, postId INTEGER REFERENCES post(ROWID));

CREATE VIRTUAL TABLE IF NOT EXISTS postFTS_1 USING fts5(content, content=tags, content_rowid=ROWID);
CREATE VIRTUAL TABLE IF NOT EXISTS postFTS_v_1 USING fts5vocab(postFTS_1, col);

CREATE VIRTUAL TABLE IF NOT EXISTS postFTS_2 USING fts5(content, content=tags, content_rowid=ROWID);
CREATE VIRTUAL TABLE IF NOT EXISTS postFTS_v_2 USING fts5vocab(postFTS_2, col);

CREATE VIRTUAL TABLE IF NOT EXISTS postFTS_5 USING fts5(content, content=tags, content_rowid=ROWID);
CREATE VIRTUAL TABLE IF NOT EXISTS postFTS_v_5 USING fts5vocab(postFTS_5, col);

-- Table: postType
CREATE TABLE IF NOT EXISTS postType (name TEXT NOT NULL UNIQUE, ROWID INTEGER PRIMARY KEY);
INSERT INTO postType (name, ROWID) VALUES ('article', 1);
INSERT INTO postType (name, ROWID) VALUES ('thought', 2);
INSERT INTO postType (name, ROWID) VALUES ('About', 3);
INSERT INTO postType (name, ROWID) VALUES ('Hero', 4);
INSERT INTO postType (name, ROWID) VALUES ('Code Snippet', 5);

-- Table: section
CREATE TABLE IF NOT EXISTS section (ROWID INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT, content TEXT, tag TEXT, createdAt INTEGER, modifiable BOOLEAN NOT NULL DEFAULT 1, contentHtml TEXT);
INSERT INTO section (ROWID, name, content, tag, createdAt, modifiable, contentHtml) VALUES (1, 'title', '', '{{title}}', NULL, 0, NULL);
INSERT INTO section (ROWID, name, content, tag, createdAt, modifiable, contentHtml) VALUES (2, 'description', '', '{{description}}', NULL, 0, NULL);
INSERT INTO section (ROWID, name, content, tag, createdAt, modifiable, contentHtml) VALUES (3, 'date', '', '{{date}}', NULL, 0, NULL);

COMMIT TRANSACTION;
PRAGMA foreign_keys = on;