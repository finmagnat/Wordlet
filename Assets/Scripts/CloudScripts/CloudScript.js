handlers.GrantStarterGift = function (args, context) {
    const gifts = {
        "boost_letter": 1,
        "boost_slow": 1,
        "boost_eraser": 1,
        "boost_swap": 1,
    };

    const keys = Object.keys(gifts);

    const getDataResult = server.GetUserReadOnlyData({
        PlayFabId: currentPlayerId,
        Keys: keys
    });

    let updateData = {};
    let totals = {};

    for (let i = 0; i < keys.length; i++) {
        const key = keys[i];
        let currentValue = 0;

        if (getDataResult.Data && getDataResult.Data[key]) {
            currentValue = parseInt(getDataResult.Data[key].Value);
        }

        const newValue = currentValue + gifts[key];
        updateData[key] = newValue.toString();
        totals[key] = newValue;
    }

    server.UpdateUserReadOnlyData({
        PlayFabId: currentPlayerId,
        Data: updateData
    });

    return {
        granted: true,
        amounts: gifts,
        totals: totals
    };
};


handlers.ConsumeBooster = function (args, context) {
    const key = args.key;
    const amount = args.amount || 1;

    const getDataResult = server.GetUserReadOnlyData({
        PlayFabId: currentPlayerId,
        Keys: [key]
    });

    let currentValue = 0;
    if (getDataResult.Data && getDataResult.Data[key]) {
        currentValue = parseInt(getDataResult.Data[key].Value);
    }

    if (currentValue < amount) {
        return { ok: false, key: key, total: currentValue };
    }

    const newValue = currentValue - amount;

    server.UpdateUserReadOnlyData({
        PlayFabId: currentPlayerId,
        Data: { [key]: newValue.toString() }
    });

    return { ok: true, key: key, total: newValue };
};

handlers.AddBooster = function (args, context) {
    const key = args.key;
    const amount = args.amount || 1;

    const result = server.GetUserReadOnlyData({
        PlayFabId: currentPlayerId,
        Keys: [key]
    });

    let current = 0;
    if (result.Data && result.Data[key]) {
        current = parseInt(result.Data[key].Value);
    }

    const total = current + amount;

    server.UpdateUserReadOnlyData({
        PlayFabId: currentPlayerId,
        Data: { [key]: total.toString() }
    });

    return { ok: true, key: key, total: total };
};

handlers.GrantPack = function (args, context) {
    // args: { source, productId, receiptJson, signature, currencyCode, purchasePrice, transactionId, debugSecret }

    var source = args.source || "";
    var productId = args.productId;
    var transactionId = args.transactionId || "";

    // --- DEBUG gate ---
    if (source === "debug") {
        var ti = server.GetTitleInternalData({ Keys: ["debug_shop_secret", "debug_shop_allowlist"] });

        var secret = ti.Data && ti.Data.debug_shop_secret ? ti.Data.debug_shop_secret : "";
        if (!secret || args.debugSecret !== secret)
            throw "Debug purchase is disabled (bad secret)";

        // optional allowlist
        if (ti.Data && ti.Data.debug_shop_allowlist) {
            var allow = JSON.parse(ti.Data.debug_shop_allowlist);
            if (allow && allow.length > 0 && allow.indexOf(currentPlayerId) < 0)
                throw "Debug purchase not allowed for this player";
        }
    }
    else if (source === "google") {
        // --- REAL validate ---
        server.ValidateGooglePlayPurchase({
            ReceiptJson: args.receiptJson,
            Signature: args.signature,
            CurrencyCode: args.currencyCode || "USD",
            PurchasePrice: args.purchasePrice || 0
        });
    }
    else {
        throw "Unknown purchase source";
    }

    // --- special: remove interstitial ads entitlement ---
    if (productId === "remove_interstitial_ads") {
        server.UpdateUserData({
            PlayFabId: currentPlayerId,
            Data: { "no_interstitial_ads": "1" }
        });

        return { ok: true, productId: productId, entitlement: "no_interstitial_ads" };
    }

    // --- rewards from TitleData ---
    var td = server.GetTitleData({ Keys: ["shop_rewards"] });
    if (!td.Data || !td.Data.shop_rewards)
        throw "TitleData shop_rewards is missing";

    var rewardsMap = JSON.parse(td.Data.shop_rewards);
    var rewards = rewardsMap[productId];
    if (!rewards)
        throw "Unknown productId: " + productId;

    // --- idempotency by transactionId (для google обязательно) ---
    // Для debug можно тоже, но не критично.
    if (transactionId) {
        var tokenKey = "purchase_tokens";
        var internal = server.GetUserInternalData({ PlayFabId: currentPlayerId, Keys: [tokenKey] });
        var used = [];
        if (internal.Data && internal.Data[tokenKey] && internal.Data[tokenKey].Value) {
            try { used = JSON.parse(internal.Data[tokenKey].Value); } catch (e) { used = []; }
        }
        if (used.indexOf(transactionId) >= 0) {
            return { ok: true, duplicate: true, productId: productId };
        }
        used.unshift(transactionId);
        if (used.length > 50) used = used.slice(0, 50);
        server.UpdateUserInternalData({ PlayFabId: currentPlayerId, Data: { [tokenKey]: JSON.stringify(used) } });
    }

    // --- apply rewards to UserReadOnlyData ---
    var keys = Object.keys(rewards);
    var cur = server.GetUserReadOnlyData({ PlayFabId: currentPlayerId, Keys: keys });

    var update = {};
    var totals = {};
    for (var i = 0; i < keys.length; i++) {
        var k = keys[i];
        var add = parseInt(rewards[k]) || 0;

        var base = 0;
        if (cur.Data && cur.Data[k]) base = parseInt(cur.Data[k].Value);

        var total = base + add;
        update[k] = total.toString();
        totals[k] = total;
    }

    server.UpdateUserReadOnlyData({ PlayFabId: currentPlayerId, Data: update });

    return { ok: true, duplicate: false, productId: productId, totals: totals };
};

//***************************************
function normalizeLanguage(language) {
    if (!language) return "ru";
    return String(language).trim().toLowerCase();
}

function normalizeWord(word) {
    if (!word) return null;

    var value = String(word).trim().toUpperCase();

    if (!value || value.length < 2 || value.length > 32)
        return null;

    if (value.indexOf(" ") >= 0)
        return null;

    return value;
}

//*************************************** NEW WORDS
var PendingWordsKey = "pending_words_";

function makeStorageKey(key, language) {
    return key + normalizeLanguage(language);
}

function loadPendingWordsCollection(language) {
    var key = makeStorageKey(PendingWordsKey, language);

    var result = server.GetTitleInternalData({
        Keys: [key]
    });

    if (!result.Data || !result.Data[key]) {
        return { words: [] };
    }

    try {
        var parsed = JSON.parse(result.Data[key]);
        if (!parsed || !parsed.words)
            return { words: [] };

        return parsed;
    } catch (e) {
        log.error("Failed to parse pending words JSON", { key: key, error: e });
        return { words: [] };
    }
}

function savePendingWordsCollection(language, collection) {
    var key = makeStorageKey(PendingWordsKey, language);

    server.SetTitleInternalData({
        Key: key,
        Value: JSON.stringify(collection)
    });
}

handlers.AddPendingWord = function (args, context) {
    var language = normalizeLanguage(args && args.language);
    var normalizedWord = normalizeWord(args && args.word);

    if (!normalizedWord) {
        return JSON.stringify({
            success: false,
            status: "Invalid",
            normalizedWord: null
        });
    }

    var collection = loadPendingWordsCollection(language);

    for (var i = 0; i < collection.words.length; i++) {
        if (collection.words[i].word === normalizedWord) {
            return JSON.stringify({
                success: true,
                status: "AlreadyExists",
                normalizedWord: normalizedWord
            });
        }
    }

    collection.words.push({
        word: normalizedWord
    });

    savePendingWordsCollection(language, collection);

    return JSON.stringify({
        success: true,
        status: "Added",
        normalizedWord: normalizedWord
    });
};

handlers.GetPendingWords = function (args, context) {
    var language = normalizeLanguage(args && args.language);
    var collection = loadPendingWordsCollection(language);

    return JSON.stringify({
        success: true,
        language: language,
        words: collection.words || []
    });
};

handlers.DeletePendingWord = function (args, context) {
    var language = normalizeLanguage(args && args.language);
    var normalizedWord = normalizeWord(args && args.word);

    if (!normalizedWord) {
        return JSON.stringify({
            success: false,
            status: "Invalid",
            normalizedWord: null
        });
    }

    var collection = loadPendingWordsCollection(language);
    var originalCount = collection.words.length;

    collection.words = collection.words.filter(function (item) {
        return item.word !== normalizedWord;
    });

    if (collection.words.length === originalCount) {
        return JSON.stringify({
            success: true,
            status: "NotFound",
            normalizedWord: normalizedWord
        });
    }

    savePendingWordsCollection(language, collection);

    return JSON.stringify({
        success: true,
        status: "Deleted",
        normalizedWord: normalizedWord
    });
};

handlers.ClearPendingWords = function(args, context) {

    var language = normalizeLanguage(args && args.language);
    var key = makeStorageKey(PendingWordsKey, language);

    server.SetTitleInternalData({
        Key: key,
        Value: JSON.stringify({ words: [] })
    });

    return JSON.stringify({
        success: true
    });
};

//*************************************** REPORT WORDS
var ReportWordsKey = "report_words_";

function loadReportWordsCollection(language) {
    var key = makeStorageKey(ReportWordsKey, language);

    var result = server.GetTitleInternalData({
        Keys: [key]
    });

    if (!result.Data || !result.Data[key]) {
        return { words: [] };
    }

    try {
        var parsed = JSON.parse(result.Data[key]);
        if (!parsed || !parsed.words)
            return { words: [] };

        return parsed;
    } catch (e) {
        log.error("Failed to parse pending words JSON", { key: key, error: e });
        return { words: [] };
    }
}

function saveReportWordsCollection(language, collection) {
    var key = makeStorageKey(ReportWordsKey, language);

    server.SetTitleInternalData({
        Key: key,
        Value: JSON.stringify(collection)
    });
}

handlers.AddReportWord = function (args, context) {
    var language = normalizeLanguage(args && args.language);
    var reason = args.reason;
    var normalizedWord = normalizeWord(args && args.word);

    if (!normalizedWord) {
        return JSON.stringify({
            success: false,
            status: "Invalid",
            normalizedWord: null
        });
    }

    var collection = loadReportWordsCollection(language);

    for (var i = 0; i < collection.words.length; i++) {
        if (collection.words[i].word === normalizedWord) {
            return JSON.stringify({
                success: true,
                status: "AlreadyExists",
                normalizedWord: normalizedWord
            });
        }
    }

    collection.words.push({
        word: normalizedWord,
        reason: reason
    });

    saveReportWordsCollection(language, collection);

    return JSON.stringify({
        success: true,
        status: "Added",
        normalizedWord: normalizedWord
    });
};

handlers.GetReportWords = function (args, context) {
    var language = normalizeLanguage(args && args.language);
    var collection = loadReportWordsCollection(language);

    return JSON.stringify({
        success: true,
        language: language,
        words: collection.words || []
    });
};

handlers.DeleteReportWord = function (args, context) {
    var language = normalizeLanguage(args && args.language);
    var normalizedWord = normalizeWord(args && args.word);

    if (!normalizedWord) {
        return JSON.stringify({
            success: false,
            status: "Invalid",
            normalizedWord: null
        });
    }

    var collection = loadReportWordsCollection(language);
    var originalCount = collection.words.length;

    collection.words = collection.words.filter(function (item) {
        return item.word !== normalizedWord;
    });

    if (collection.words.length === originalCount) {
        return JSON.stringify({
            success: true,
            status: "NotFound",
            normalizedWord: normalizedWord
        });
    }

    saveReportWordsCollection(language, collection);

    return JSON.stringify({
        success: true,
        status: "Deleted",
        normalizedWord: normalizedWord
    });
};

handlers.ClearReportWords = function(args, context) {

    var language = normalizeLanguage(args && args.language);
    var key = makeStorageKey(ReportWordsKey, language);

    server.SetTitleInternalData({
        Key: key,
        Value: JSON.stringify({ words: [] })
    });

    return JSON.stringify({
        success: true
    });
};