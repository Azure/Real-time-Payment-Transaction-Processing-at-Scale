function processTransaction(transaction) {
    var collection = getContext().getCollection();

    var accountId = collection.getAltLink() + '/docs/' + transaction.accountId;

    // Read account document
    var isAccepted = collection.readDocument(
        accountId, {},
        function (err, accountSummary) {
            if (err) throw err;

            // Check operation type
            if (transaction.type.toLowerCase() == "debit") {
                if ((accountSummary.balance + accountSummary.limit) < transaction.amount) {
                    throw "Insufficient balance/limit!";
                }
                else {
                    accountSummary.balance -= transaction.amount;
                }
            }
            else if (transaction.type.toLowerCase() == "deposit") {
                accountSummary.balance += transaction.amount;
            }

            // Provide eTag to handle concurrency
            var requestOptions = { etag: accountSummary._etag };

            // Replace account document with updated balance
            var isAccepted = collection.replaceDocument(accountSummary._self, accountSummary, requestOptions, function (err, updatedDocument) {
                if (err) throw err;

                // Create transaction document once replace succeeded
                var isAccepted = collection.createDocument(collection.getSelfLink(), transaction, function (err, doc) {
                    if (err) throw err;
                });

                // Return update account details
                getContext().getResponse().setBody(accountSummary);
            });

        });

    if (!isAccepted) throw new Error('The query was not accepted by the server.');
};