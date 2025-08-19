import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
    wrapper: {
        display: 'grid',
        gap: 12,
        gridTemplateColumns: '1fr 200px 200px',
        alignItems: 'start',
        marginBottom: 16,
        minHeight: 0
    },
    label: {
        display: 'block',
        fontSize: 12,
        color: '#555'
    },
    input: {
        width: '95%', 
        padding: 8, 
        borderRadius: 8, 
        border: '1px solid #ccc'
    },
    select: {
        width: '100%', 
        padding: 8, 
        borderRadius: 8, 
        border: '1px solid #ccc'
    }


});
