function num2unit(sz)
{
     if(sz < 1024)
     {
        return `${sz}`;
     }
     if(sz < (1024*1024))
     {
        return `${(sz / 1024).toFixed()}K`
     }
     if(sz < (1024*1024*1024))
     {
        return `${(sz / (1024 * 1024)).toFixed()}M`
     }
     if(sz < (1024*1024*1024*1024))
     {
        return `${(sz / (1024 * 1024 * 1024)).toFixed()}G`
     }
     return `${(sz / (1024 * 1024 * 1024 * 1024)).toFixed()}T`
}