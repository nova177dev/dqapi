create table dbo.user_roles
(
    id tinyint not null primary key,
    uuid uniqueidentifier not null
        index ix_user_roles_uuid
        constraint df_user_roles_uuid default (newid()),
    role_name varchar(128) not null
)
