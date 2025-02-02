create table dbo.user_statuses
(
    id tinyint not null primary key,
    uuid uniqueidentifier not null
        index ix_user_statuses_uuid
        constraint df_user_statuses_uuid default (newid()),
    status_name varchar(128)
)
