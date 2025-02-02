create table dbo.error_log
(
    id bigint identity(1,1) not null primary key,
    uuid varchar(128)
        index ix_error_log_uuid,
    created_dtm datetime2
        index ix_error_log_created_dtm
        constraint df_error_log_created_dtm default (getutcdate()),
    causer varchar(128) null
        index ix_error_log_causer,
    params nvarchar(max) null,
    error nvarchar(max) null
)