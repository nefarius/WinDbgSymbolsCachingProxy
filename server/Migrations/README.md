# MongoDB Migrations

Schema migrations in this folder perform in-place fixes for schema changes over the development lifecycle of the app.

They are applied during server startup through MongoDB.Entities before indexes are ensured. If you are starting from
scratch, there is usually no legacy data to migrate, so these scripts won't change anything.
