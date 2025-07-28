import { useNavigate } from 'react-router-dom';
import { useStyles } from './style';

export const HomePage = () => {
	const classes = useStyles();
	const navigate = useNavigate();

	const handleNavigation = (path: string) => {
		navigate(path);
	};

	return (
		<div className={classes.homePageWrapper}>
			<h1>Welcome to our internal playground project</h1>

			<div className={classes.columnsWrapper}>
				{/* Chat Tools Column */}
				<div className={classes.column}>
					<h2>Chat Tools</h2>
					<button
						className={classes.button}
						onClick={() => handleNavigation('/chat/sh')}
					>
						Chat - Sh (Shirley - OpenAI)
					</button>
					<button
						className={classes.button}
						onClick={() => handleNavigation('/chat/yo')}
					>
						Chat - Yo (Yonatan)
					</button>
					<button
						className={classes.button}
						onClick={() => handleNavigation('/chat/da')}
					>
						Chat - Da (Daniel)
					</button>
					<button
						className={classes.button}
						onClick={() => handleNavigation('/chat/ou')}
					>
						Chat - Ou (Ouriel)
					</button>
				</div>

				{/* Avatar Tools Column */}
				<div className={classes.column}>
					<h2>Avatar Tools</h2>
					<button
						className={classes.button}
						onClick={() => handleNavigation('/avatar/ou')}
					>
						Avatar - Ou (Ouriel)
					</button>
					<button
						className={classes.button}
						onClick={() => handleNavigation('/avatar/sh')}
					>
						Avatar - Sh (Shirley)
					</button>
					<button
						className={classes.button}
						onClick={() => handleNavigation('/avatar/da')}
					>
						Avatar - Da (Daniel)
					</button>
				</div>
				{/* Practices Tools Column*/}
				<div className={classes.column}>
					<h2>Practices Tools</h2>
					<button
						className={classes.button}
						onClick={() => handleNavigation('typing')}
					>
						Typing Practice
					</button>
					<button
						className={classes.button}
						onClick={() => handleNavigation('speaking')}
					>
						Speaking Practice
					</button>
				</div>
			</div>
		</div>
	);
};
