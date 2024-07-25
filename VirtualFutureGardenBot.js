import express from "express";
import path from "path";
import { Telegraf } from "telegraf";
import { connectHereWallet } from "./here-connector.js";
import crypto from "crypto";
import { sha256 } from "js-sha256";
import { fileURLToPath } from 'url';
import { dirname } from 'path';

const TOKEN = "my_tg_token(u_can_get_it_from_bot_father)";
const bot = new Telegraf(TOKEN);
const server = express();
const port = process.env.PORT || 5001;
const gameName = "VirtualFutureGarden";
const queries = {};
const requiredChannel = '@virtualfuturegarden';
const referrals = {};
const referralCount = {};
const userNames = {};
const adminRefId = "my_admin_ref_if(u_can_get_it_from_ref_link_or_from_logs)";


const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);


server.use(express.static(path.join(__dirname, 'NewGameWebBackend')));


async function isUserSubscribed(userId) {
    try {
        const chatMember = await bot.telegram.getChatMember(requiredChannel, userId);
        return chatMember.status === 'member' || chatMember.status === 'administrator' || chatMember.status === 'creator';
    } catch (error) {
        console.error('Error checking subscription status:', error);
        return false;
    }
}

function sendSubscriptionMessage(userId) {
    bot.telegram.sendMessage(userId, `Please subscribe to our channel first: ${requiredChannel}`, {
        reply_markup: {
            inline_keyboard: [[{ text: "Done", callback_data: "check_subscription" }]]
        }
    });
}


bot.command("start", async (ctx) => {
    const chatId = ctx.chat.id;
    const args = ctx.message.text.split(' ');
    const refId = args[1] ? args[1].replace('ref', '') : adminRefId;

    if (!referralCount[refId]) {
        referralCount[refId] = 0;
    }

    const isSubscribed = await isUserSubscribed(ctx.from.id);

    if (isSubscribed) {
        if (referrals[refId]) {
            const inviterId = referrals[refId].inviter;
            if (inviterId !== ctx.from.id && !referrals[refId].invitees.includes(ctx.from.id)) {
                referrals[refId].invitees.push(ctx.from.id);
                referralCount[inviterId] = (referralCount[inviterId] || 0) + 1;
                userNames[ctx.from.id] = ctx.from.first_name || "Unknown"; 
                bot.telegram.sendMessage(inviterId, `You have a new referral: ${userNames[ctx.from.id]}`);
                bot.telegram.sendMessage(ctx.from.id, "Thanks for joining via a referral link!");
            }
        } else {
            referrals[adminRefId] = referrals[adminRefId] || { inviter: adminRefId, invitees: [] };
            if (!referrals[adminRefId].invitees.includes(ctx.from.id)) {
                referrals[adminRefId].invitees.push(ctx.from.id);
                referralCount[adminRefId] = (referralCount[adminRefId] || 0) + 1;
                userNames[ctx.from.id] = ctx.from.first_name || "Unknown"; 
                bot.telegram.sendMessage(adminRefId, `You have a new referral: ${userNames[ctx.from.id]}`);
                bot.telegram.sendMessage(ctx.from.id, "Thanks for joining the game!");
            }
        }
        ctx.reply("Send /login to authorize");
    } else {
        sendSubscriptionMessage(chatId);
    }
});

bot.command("login", (ctx) => {
    connectHereWallet(ctx, {
        nonce: Array.from(crypto.randomBytes(32)),
        recipient: "VirtualFutureGarden",
        message: "Authorization",
        type: "sign",
    })
    .then((result) => {
        console.log('Authorization result:', result);

        const accountId = result.account_id;
        const token = sha256(accountId + 'uniqueSecretKey');
        const gameUrl = `https://yegmina.github.io/VirtualFutureGarden?accountId=${accountId}&token=${token}`;

        ctx.reply("Click the button to play the game", {
            reply_markup: {
                inline_keyboard: [[
                    {
                        text: "Play NewGame",
                        web_app: {
                            url: gameUrl
                        }
                    }
                ]]
            }
        });


    })
    .catch((error) => {
        console.error('Authorization failed:', error);
        ctx.reply(`Authorization failed`);
    });
});

bot.command("referral", async (ctx) => {
    const isSubscribed = await isUserSubscribed(ctx.from.id);
    if (!isSubscribed) {
        sendSubscriptionMessage(ctx.from.id);
        return;
    }

    const refId = ctx.from.id.toString();
    referrals[refId] = referrals[refId] || { inviter: ctx.from.id, invitees: [] };
    userNames[ctx.from.id] = ctx.from.first_name || "Unknown";
    const referralLink = `https://t.me/VirtualFutureGarden_bot?start=ref${refId}`;
    ctx.reply(`Invite your friends using this link: ${referralLink}`);
});

bot.on("callback_query", async (query) => {
    if (query.data === "check_subscription") {
        const isSubscribed = await isUserSubscribed(query.from.id);
        if (isSubscribed) {
            bot.telegram.sendMessage(query.from.id, "Thank you for subscribing! You can now use /start command.");
        } else {
            sendSubscriptionMessage(query.from.id);
        }
    }
});


bot.command("leaderboard", (ctx) => {
    const sortedReferrals = Object.entries(referralCount).sort((a, b) => b[1] - a[1]);
    let leaderboardMessage = "Referral Leaderboard:\n\n";

    sortedReferrals.forEach(([userId, count], index) => {
        const userName = userNames[userId] || userId;
        leaderboardMessage += `${index + 1}. ${userName}: ${count} referrals\n`;
    });

    ctx.reply(leaderboardMessage);
});


bot.command("referrals", (ctx) => {
    const userId = ctx.from.id.toString();
    if (referrals[userId]) {
        const inviterData = referrals[userId];
        let message = `You have referred ${inviterData.invitees.length} users:\n`;
        inviterData.invitees.forEach((inviteeId, index) => {
            const inviteeName = userNames[inviteeId] || "Unknown";
            message += `${index + 1}. ${inviteeName}\n`;
        });
        ctx.reply(message);
    } else {
        ctx.reply("You have not referred any users yet.");
    }
});


bot.command("help", (ctx) => {
    ctx.reply("You can use /login, /referral, /leaderboard, /referrals commands");
});


bot.command("push", (ctx) => {
    const args = ctx.message.text.split(' ');
    const target = args[1];
    const message = args.slice(2).join(' ');

    if (ctx.from.id.toString() !== adminRefId) {
        ctx.reply("You are not authorized to use this command.");
        return;
    }

    if (target === "everyone") {
        Object.keys(userNames).forEach(userId => {
            bot.telegram.sendMessage(userId, message);
        });
    } else if (target.match(/^\d+$/)) {
        bot.telegram.sendMessage(target, message);
    } else {
        ctx.reply("Invalid command format.");
    }
});

bot.command("stat", (ctx) => {
    if (ctx.from.id.toString() !== adminRefId) {
        ctx.reply("You are not authorized to use this command.");
        return;
    }

    let message = "User Statistics:\n";
    for (const userId in referrals) {
        const inviterData = referrals[userId];
        const userName = userNames[userId] || "Unknown";
        const refCount = inviterData.invitees.length;
        const refFather = inviterData.inviter;
        message += `${userName} - ${userId} - ${refCount} referrals - Referred by ${refFather}\n`;
    }
    ctx.reply(message);
});

bot.launch();

server.listen(port, () => {
    console.log(`Server is running on port ${port}`);
});
