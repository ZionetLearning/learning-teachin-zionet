import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
    gameContainer: {
        flexDirection: "row"
    },
    speakersContainer: {
        display: "flex",
        flexDirection: "row",
        alignItems: "center",
        justifyContent: "center",
    },
    sideButtons: {
        flexDirection: "column",
    },
    wordsBank: {
        display: "flex",
        flexDirection: "row",
    }
});
