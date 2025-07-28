import { useStyles } from "./style";
export const Header = () => {
    const classes = useStyles();
    return (<h1 className={classes.header}>Word Order Game</h1>);
}