const { Sequelize, sequelize } = require('./lib/database');

async function seedData(targetSchema) {
  const TOTAL_RECORDS = 1000000;
  const BATCH_SIZE = 5000;
  const tableName = 'stub_data';

  console.log(`Starting to seed ${TOTAL_RECORDS} records into ${targetSchema}.${tableName}...`);

  const paths = ['/api/v1/user', '/api/v1/order', '/api/v1/product', '/api/v1/auth/login', '/api/v1/payment'];
  
  try {
    // Chuyển sang schema mong muốn
    await sequelize.query(`USE \`${targetSchema}\``);

    let count = 0;
    while (count < TOTAL_RECORDS) {
      const records = [];
      for (let i = 0; i < BATCH_SIZE && (count + i) < TOTAL_RECORDS; i++) {
        const id = count + i + 1;
        const path = paths[Math.floor(Math.random() * paths.length)] + '/' + Math.floor(Math.random() * 1000);
        
        records.push({
          path: path,
          input_json: JSON.stringify({ id: id, query: `staging_query_${id}`, timestamp: Date.now() }),
          output_json: JSON.stringify({ status: "success", env: "staging", data: { id: id, name: `Staging Item ${id}` } }),
          header_json: JSON.stringify({ "content-type": "application/json" }),
          http_status_cd: 200,
          response_add_time: Math.floor(Math.random() * 50),
          memo: `Staging Seed #${id}`
        });
      }

      const values = records.map(r => 
        `(${sequelize.escape(r.path)}, ${sequelize.escape(r.input_json)}, ${sequelize.escape(r.output_json)}, ${sequelize.escape(r.header_json)}, ${r.http_status_cd}, ${r.response_add_time}, ${sequelize.escape(r.memo)})`
      ).join(',');

      await sequelize.query(`INSERT INTO \`${tableName}\` (path, input_json, output_json, header_json, http_status_cd, response_add_time, memo) VALUES ${values}`);

      count += BATCH_SIZE;
      process.stdout.write(`Staging Progress: ${((count / TOTAL_RECORDS) * 100).toFixed(2)}% (${count}/${TOTAL_RECORDS})\r`);
    }

    console.log(`\n✅ Seeding for ${targetSchema} completed!`);
    process.exit(0);
  } catch (error) {
    console.error('\n❌ Error:', error);
    process.exit(1);
  }
}

// Chạy cho staging
seedData('staging');
