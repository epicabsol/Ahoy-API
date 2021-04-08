/*
 * Drops and recreates all the tables for the Ahoy API.
 */

/* Drop all the tables if they exist */
DROP TABLE IF EXISTS
    Post;

/* Create the Post table */
CREATE TABLE Post (
    ID              INT GENERATED ALWAYS AS IDENTITY,
    AuthorName      VARCHAR(50) NOT NULL,
    Content         VARCHAR(280) NOT NULL,

    CONSTRAINT Post_PK
        PRIMARY KEY (ID)
);