import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
    chatWrapper: {
        maxWidth: "800px",
        maxHeight: "800px",
        margin: "auto",
        border: "1px solid #ccc",
        padding: 10
    },
    messagesList: {
        height: "500px",
        overflowX: "auto",
        overflowY: "auto",
        marginBottom: 10
    },
    messageBox: {
        "& .rce-mbox-right-notch": {
            fill: "#11bbff !important",
        },
    },
    input: {
        border: "1px solid #ddd",
        borderRadius: "0%",
        paddingLeft: "6px"
    },
    sendButton: {
        backgroundColor: "#44bbff",
        color: "#fff",
        borderRadius: "50%",
        width: "35px",
        height: "30px",
        fontSize: "22px",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        padding: 0,
        lineHeight: 1,
    }
});
