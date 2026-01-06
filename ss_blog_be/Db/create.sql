--
-- File generated with SQLiteStudio v3.4.4 on Sun Dec 28 23:48:03 2025
--
-- Text encoding used: System
--
PRAGMA foreign_keys = off;

-- Table: postType
CREATE TABLE IF NOT EXISTS postType (name TEXT NOT NULL UNIQUE, ROWID INTEGER PRIMARY KEY);
INSERT INTO postType (name, ROWID) VALUES ('article', 1);
INSERT INTO postType (name, ROWID) VALUES ('thought', 2);
INSERT INTO postType (name, ROWID) VALUES ('About', 3);
INSERT INTO postType (name, ROWID) VALUES ('Hero', 4);
INSERT INTO postType (name, ROWID) VALUES ('Code Snippet', 5);

-- Table: section
CREATE TABLE section (
    ROWID            INTEGER PRIMARY KEY AUTOINCREMENT,
    name             TEXT,
    content          TEXT,
    tag              TEXT,
    createdAt        INTEGER,
    modifiable       BOOLEAN NOT NULL DEFAULT 1
);

INSERT INTO section (name, content, tag, modifiable) VALUES ('title', '', '{{title}}', 0);
INSERT INTO section (name, content, tag, modifiable) VALUES ('description', '', '{{description}}', 0);
INSERT INTO section (name, content, tag, modifiable) VALUES ('date', '', '{{date}}', 0);

-- Table: post
CREATE TABLE post (
    title            TEXT    NOT NULL,
    content          TEXT,
    description      TEXT,
    createdAt        INTEGER,
    typeId           INTEGER REFERENCES postType (ROWID),
    ROWID            INTEGER PRIMARY KEY AUTOINCREMENT,
    isPublished      INTEGER,
    tags             TEXT,
    tagsCodeSnippets TEXT,
    previousTags     TEXT
);

-- Table: content
CREATE TABLE IF NOT EXISTS content (objId TEXT, postId INTEGER REFERENCES post (ROWID), type NUMERIC NOT NULL);

-- Table: postFTS
CREATE VIRTUAL TABLE IF NOT EXISTS postFTS USING fts5(tags, tagsCodeSnippets , content=post, content_rowid=ROWID);

---- Table: postFTS_v
CREATE VIRTUAL TABLE IF NOT EXISTS postFTS_v USING fts5vocab(postFTS, col);

COMMIT TRANSACTION;
PRAGMA foreign_keys = on;
