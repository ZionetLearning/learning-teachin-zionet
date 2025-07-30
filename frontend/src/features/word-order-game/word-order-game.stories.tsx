import { WordOrderGame } from "./index";
import type { Meta, StoryObj } from "@storybook/react-vite";

const meta: Meta = {
    title: "Features/WordOrderGame",
    component: WordOrderGame,
};

export default meta;

export const Default: StoryObj = {
    render: () => <WordOrderGame />,
};